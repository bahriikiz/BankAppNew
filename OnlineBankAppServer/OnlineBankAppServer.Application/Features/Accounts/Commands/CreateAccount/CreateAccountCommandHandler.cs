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
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        Random random = new Random();
        string ibanNumbers = string.Empty;

        for (int i = 0; i < 24; i++)
        {
            ibanNumbers += random.Next(0, 10).ToString();
        }

        string newIban = "TR" + ibanNumbers;

        var account = new Account
        {
            Iban = newIban,
            CurrencyType = request.CurrencyType,
            Balance = 0,
            BankId = request.BankId,
            UserId = userId,
            CreatedDate = DateTime.Now,
            Transactions = new List<BankTransaction>()
        };

        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);

        return $"{request.CurrencyType} hesabınız başarıyla oluşturuldu. IBAN: {newIban}";
    }
}