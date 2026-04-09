using MediatR;
using System.Text;
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
        var userIdClaim = (httpContextAccessor.HttpContext?.User.FindFirst("UserId")
                          ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")) ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        Random random = new();
        StringBuilder ibanBuilder = new();

        for (int i = 0; i < 24; i++)
        {
            ibanBuilder.Append(random.Next(0, 10));
        }

        string generatedNumbers = ibanBuilder.ToString();
        string newIban = "TR" + generatedNumbers;
        string newAccountNumber = generatedNumbers.Substring(8, 16);

        var account = new Account
        {
            Iban = newIban,
            CurrencyType = request.CurrencyType,
            Balance = 0,
            UserId = userId,
            CreatedDate = DateTime.Now,
            BankId = 1,
            ProviderBank = "İKİZ BANK",
            AccountName = string.IsNullOrWhiteSpace(request.AccountName) ? "İKİZ BANK Vadesiz Hesap" : request.AccountName,
            AccountNumber = newAccountNumber,
            AvailableBalance = 0,
            AccountType = "Vadesiz",
            IsActive = true,
            LastTransactionDate = DateTime.Now,
            Transactions = []
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        return $"İKİZ BANK {request.CurrencyType} hesabınız başarıyla oluşturuldu. IBAN: {newIban}";
    }
}