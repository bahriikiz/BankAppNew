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
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // 2. Hedef IBAN'ı Belirle
        string finalTargetIban = await ResolveTargetIbanAsync(request, userId, cancellationToken);

        // 3. Gönderen Hesabı Doğrula ve Bakiye Kontrolü Yap
        var sourceAccount = await context.Accounts
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Gönderen hesap bulunamadı veya size ait değil.");

        if (sourceAccount.Balance < request.Amount)
            throw new InvalidOperationException("Yetersiz bakiye.");

        // 4. Günlük Limit Kontrolü
        await CheckDailyLimitAsync(sourceAccount, request.Amount, cancellationToken);

        // 5. Alıcı Hesabı Doğrula
        var targetAccount = await context.Accounts
            .FirstOrDefaultAsync(x => x.Iban == finalTargetIban, cancellationToken)
            ?? throw new KeyNotFoundException("Alıcı hesap (IBAN) bulunamadı.");

        if (sourceAccount.Id == targetAccount.Id)
            throw new InvalidOperationException("Kendi kendinize para transferi yapamazsınız.");

        // 6. Döviz Çevirimi (Eğer gerekiyorsa)
        decimal amountToCredit = await CalculateCreditAmountAsync(sourceAccount, targetAccount, request.Amount, cancellationToken);

        // 7. Transfer İşlemini Gerçekleştir (Veritabanı Transaction)
        await ExecuteTransferAsync(sourceAccount, targetAccount, request.Amount, amountToCredit, request.Description, finalTargetIban, cancellationToken);

        return "Para transferi başarıyla gerçekleşti.";
    }

    // --- YARDIMCI (PRIVATE) METODLAR ---

    private async Task<string> ResolveTargetIbanAsync(MoneyTransferCommand request, int userId, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.TargetIban))
            return request.TargetIban;

        if (request.BeneficiaryId.HasValue && request.BeneficiaryId > 0)
        {
            var beneficiary = await context.Beneficiaries
                .FirstOrDefaultAsync(x => x.Id == request.BeneficiaryId && x.UserId == userId, cancellationToken)
                ?? throw new KeyNotFoundException("Rehberdeki kayıt bulunamadı.");

            return beneficiary.Iban;
        }

        throw new ArgumentException("Lütfen bir IBAN girin veya rehberden bir alıcı seçin.");
    }

    private async Task CheckDailyLimitAsync(Account sourceAccount, decimal transferAmount, CancellationToken cancellationToken)
    {
        string currencyCode = sourceAccount.CurrencyType?.Trim().ToUpper() ?? "TRY";

        decimal dailyLimit = currencyCode switch
        {
            "TRY" or "TL" or "1" => 1000000m,
            "USD" or "2" => 75000m,
            "EUR" or "3" => 50000m,
            "XAU" or "4" => 100m,
            _ => 10000m
        };

        var todaysTotalTransfer = await context.BankTransactions
            .Where(x => x.AccountId == sourceAccount.Id
                     && x.TransactionDate >= DateTime.Today
                     && x.TargetIban != sourceAccount.Iban) 
            .SumAsync(x => x.Amount, cancellationToken);

        if (todaysTotalTransfer + transferAmount > dailyLimit)
        {
            decimal remainingLimit = dailyLimit - todaysTotalTransfer;

            if (remainingLimit <= 0)
                throw new InvalidOperationException($"Günlük transfer limitinizi ({dailyLimit:N2} {currencyCode}) doldurdunuz. Yarın tekrar deneyebilirsiniz.");

            throw new InvalidOperationException($"Günlük transfer limitinizi aşıyorsunuz! Bu hesap için bugün kalan limitiniz: {remainingLimit:N2} {currencyCode}.");
        }
    }

    private async Task<decimal> CalculateCreditAmountAsync(Account sourceAccount, Account targetAccount, decimal amount, CancellationToken cancellationToken)
    {
        if (sourceAccount.CurrencyType == targetAccount.CurrencyType)
            return amount;

        string sourceCur = NormalizeCurrencyCode(sourceAccount.CurrencyType);
        string targetCur = NormalizeCurrencyCode(targetAccount.CurrencyType);

        decimal rate = await exchangeService.GetRateAsync(sourceCur, targetCur, cancellationToken);
        return amount * rate;
    }

    private static string NormalizeCurrencyCode(string? currencyType)
    {
        return currencyType?.Trim().ToUpper() switch
        {
            "1" => "TRY",
            "2" => "USD",
            "3" => "EUR",
            "4" => "XAU",
            var c => c ?? "TRY"
        };
    }

    private async Task ExecuteTransferAsync(Account source, Account target, decimal amountToDebit, decimal amountToCredit, string description, string targetIban, CancellationToken cancellationToken)
    {
        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            source.Balance -= amountToDebit;
            target.Balance += amountToCredit;

            string finalDescription = description;
            if (source.CurrencyType != target.CurrencyType)
            {
                finalDescription += $" (Döviz Dönüşümü: {source.CurrencyType} -> {target.CurrencyType})";
            }

            context.BankTransactions.Add(new BankTransaction
            {
                AccountId = source.Id,
                Amount = amountToDebit,
                Description = finalDescription,
                TargetIban = targetIban,
                TransactionDate = DateTime.Now,
                CreatedDate = DateTime.Now
            });

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}