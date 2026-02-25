using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Integration.Vakifbank;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;

public sealed class SyncVakifbankAccountsCommandHandler : IRequestHandler<SyncVakifbankAccountsCommand, SyncVakifbankAccountsCommandResponse>
{
    private readonly IVakifbankService _vakifbankService;
    private readonly AppDbContext _context;

    public SyncVakifbankAccountsCommandHandler(
        IVakifbankService vakifbankService,
        AppDbContext context)
    {
        _vakifbankService = vakifbankService;
        _context = context;
    }

    public async Task<SyncVakifbankAccountsCommandResponse> Handle(SyncVakifbankAccountsCommand request, CancellationToken cancellationToken)
    {
        // ARTIK FRONTEND'DEN GELEN RIZA NO'YI VAKIFBANK'A GÖNDER!
        var vakifbankResponse = await _vakifbankService.GetAccountsAsync(request.RizaNo, cancellationToken);

        if (vakifbankResponse?.Data?.Accounts == null || !vakifbankResponse.Data.Accounts.Any())
        {
            return new SyncVakifbankAccountsCommandResponse(false, "Vakıfbank'tan hesap bilgisi alınamadı veya Rıza No geçersiz.");
        }

        foreach (var vAccount in vakifbankResponse.Data.Accounts)
        {
          
            bool isTakenBySomeoneElse = await _context.Accounts
                .AnyAsync(a => a.Iban == vAccount.IBAN && a.UserId != request.UserId, cancellationToken);

            if (isTakenBySomeoneElse)
            {
                throw new ArgumentException($"Güvenlik İhlali: {vAccount.IBAN} numaralı hesap zaten başka bir müşteriye tanımlı! Başkasına ait hesapları bağlayamazsınız.");
            }

            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Iban == vAccount.IBAN && a.UserId == request.UserId, cancellationToken);

            _ = decimal.TryParse(vAccount.Balance, out decimal currentBalance);
            _ = decimal.TryParse(vAccount.RemainingBalance, out decimal availableBalance);

            if (existingAccount != null)
            {
                existingAccount.Balance = currentBalance;
                existingAccount.AvailableBalance = availableBalance;
                existingAccount.IsActive = vAccount.AccountStatus == "A";
                existingAccount.LastTransactionDate = vAccount.LastTransactionDate;
            }
            else
            {
                var newAccount = new Account
                {
                    UserId = request.UserId,
                    AccountName = "Vakıfbank Hesabı",
                    AccountNumber = vAccount.AccountNumber,
                    Iban = vAccount.IBAN,
                    Balance = currentBalance,
                    AvailableBalance = availableBalance,
                    CurrencyType = vAccount.CurrencyCode,
                    ProviderBank = "Vakifbank",
                    AccountType = vAccount.AccountType,
                    IsActive = vAccount.AccountStatus == "A",
                    LastTransactionDate = vAccount.LastTransactionDate,
                    BankId = 1,
                    Transactions = new List<BankTransaction>()
                };

                await _context.Accounts.AddAsync(newAccount, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return new SyncVakifbankAccountsCommandResponse(true, "Vakıfbank hesapları başarıyla senkronize edildi.");
    }
}