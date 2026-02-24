using MediatR;
using Microsoft.AspNetCore.Http;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.CreateAccount;

internal sealed class CreateAccountCommandHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateAccountCommand, string>
{
    public async Task<string> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // JWT Token'dan kullanıcı kimliğini alıyoruz
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("UserId")
                          ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub");

        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        Random random = new Random();
        string ibanNumbers = string.Empty;

        // 24 haneli rastgele IBAN numarası üretimi
        for (int i = 0; i < 24; i++)
        {
            ibanNumbers += random.Next(0, 10).ToString();
        }

        string newIban = "TR" + ibanNumbers;

        // Gerçekçi bir hesap numarası için IBAN'ın son 16 hanesini 
        string newAccountNumber = ibanNumbers.Substring(8, 16);

        var account = new Account
        {
            Iban = newIban,
            CurrencyType = request.CurrencyType,
            Balance = 0,
            UserId = userId,
            CreatedDate = DateTime.Now,

            // --- AÇIK BANKACILIK & İKİZ BANK KONTROLLERİ ---
            BankId = 1,
            ProviderBank = "İKİZ BANK",
            AccountName = "İKİZ BANK Vadesiz Hesap",
            AccountNumber = newAccountNumber,
            AvailableBalance = 0,
            AccountType = "Vadesiz",
            IsActive = true,
            LastTransactionDate = DateTime.Now,

            Transactions = new List<BankTransaction>()
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        return $"İKİZ BANK {request.CurrencyType} hesabınız başarıyla oluşturuldu. IBAN: {newIban}";
    }
}