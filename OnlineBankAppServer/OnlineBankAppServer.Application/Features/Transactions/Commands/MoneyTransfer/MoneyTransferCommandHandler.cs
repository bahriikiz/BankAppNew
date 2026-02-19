using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Transactions.Commands.MoneyTransfer;

internal sealed class MoneyTransferCommandHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IExchangeService exchangeService) : IRequestHandler<MoneyTransferCommand, string>
{
    public async Task<string> Handle(MoneyTransferCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı Doğrula
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // --- IBAN Belirleme ---
        string? finalTargetIban = request.TargetIban;

        if (string.IsNullOrEmpty(finalTargetIban) && request.BeneficiaryId.HasValue && request.BeneficiaryId > 0)
        {
            var beneficiary = await context.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryId && x.UserId == userId, cancellationToken);

            if (beneficiary is null) throw new Exception("Rehberdeki kayıt bulunamadı.");
            finalTargetIban = beneficiary.Iban;
        }

        if (string.IsNullOrEmpty(finalTargetIban))
            throw new Exception("Lütfen bir IBAN girin veya rehberden bir alıcı seçin.");

        // 2. Gönderen Hesabı Bul
        var sourceAccount = await context.Accounts
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken);

        if (sourceAccount is null)
            throw new Exception("Gönderen hesap bulunamadı veya size ait değil.");

        // 3. Bakiye Kontrolü
        if (sourceAccount.Balance < request.Amount)
            throw new Exception("Yetersiz bakiye.");
        // 3.1 Transfer Limiti 
        decimal dailyLimit = sourceAccount.CurrencyType == "1" ? 100000m : 3000m;

        var today = DateTime.Today;

        // Bu hesaptan bugün yapılan transferleri çek
        var todaysTotalTransfer = await context.BankTransactions
            .Where(x => x.AccountId == sourceAccount.Id && x.TransactionDate >= today)
            .SumAsync(x => x.Amount, cancellationToken);

        // Eğer bugünkü harcamalar + şu an gönderilmek istenen tutar limiti aşıyorsa hata fırlat
        if (todaysTotalTransfer + request.Amount > dailyLimit)
        {
            decimal remainingLimit = dailyLimit - todaysTotalTransfer;
            string currencyCode = sourceAccount.CurrencyType == "1" ? "TRY" : "USD";

            // Eğer daha önceden limiti doldurduysa farklı, doldurmadıysa farklı mesaj verelim
            if (remainingLimit <= 0)
                throw new Exception($"Günlük transfer limitinizi ({dailyLimit:N2} {currencyCode}) doldurdunuz. Yarın tekrar deneyebilirsiniz.");
            else
                throw new Exception($"Günlük transfer limitinizi aşıyorsunuz! Bu hesap için bugün kalan limitiniz: {remainingLimit:N2} {currencyCode}.");
        }

        // 4. Alıcı Hesabı Bul
        var targetAccount = await context.Accounts
            .FirstOrDefaultAsync(x => x.Iban == finalTargetIban, cancellationToken);

        if (targetAccount is null)
            throw new Exception("Alıcı hesap (IBAN) bulunamadı.");

        if (sourceAccount.Id == targetAccount.Id)
            throw new Exception("Kendi kendinize para transferi yapamazsınız.");

        // --- DÖVİZ HESAPLAMA MANTIĞI ---
        decimal amountToDebit = request.Amount; // Gönderenden düşecek tutar (Kendi para birimi)
        decimal amountToCredit = request.Amount; // Alıcıya geçecek tutar

        // Para birimleri farklıysa canlı kuru al
        if (sourceAccount.CurrencyType != targetAccount.CurrencyType)
        {
            decimal rate = await exchangeService.GetRateAsync(
                sourceAccount.CurrencyType.ToString(),
                targetAccount.CurrencyType.ToString(),
                cancellationToken);

            amountToCredit = request.Amount * rate;
        }

        // --- (ATOMIC TRANSACTION) ---
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // A. Bakiyeleri Güncelle
            sourceAccount.Balance -= amountToDebit;
            targetAccount.Balance += amountToCredit;

            // B. İşlem Kaydı Oluştur 
            string description = request.Description;
            if (sourceAccount.CurrencyType != targetAccount.CurrencyType)
            {
                description += $" (Döviz Dönüşümü: {sourceAccount.CurrencyType} -> {targetAccount.CurrencyType})";
            }

            BankTransaction bankTransaction = new()
            {
                AccountId = sourceAccount.Id,
                Amount = amountToDebit,
                Description = description,
                TargetIban = finalTargetIban,
                TransactionDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };

            context.BankTransactions.Add(bankTransaction);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return "Para transferi başarıyla gerçekleşti.";
    }
}