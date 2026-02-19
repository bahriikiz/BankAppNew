using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.DTOs;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetById;

internal sealed class GetAccountByIdQueryHandler(
    AppDbContext context) : IRequestHandler<GetAccountByIdQuery, AccountDetailDto>
{
    public async Task<AccountDetailDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Hesabı Bul
        var account = await context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == request.UserId, cancellationToken);

        if (account is null)
        {
            throw new Exception("Hesap bulunamadı veya bu işlem için yetkiniz yok!");
        }

        // 2. Giden Paralar (Benim gönderdiklerim)
        var outgoingTransactions = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.AccountId == account.Id)
            .Select(x => new AccountTransactionDto(
                x.Amount,
                x.Description ?? "Transfer",
                x.TransactionDate,
                x.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"), // 4. Parametre: TR Saati Formatlı
                "Giden Para",
                x.TargetIban ?? "Bilinmiyor"
            ))
            .ToListAsync(cancellationToken);

        // 3. Gelen Paralar (Bana gelenler)
        var incomingTransactionsRaw = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.TargetIban == account.Iban)
            .ToListAsync(cancellationToken);

        // Gelen transferlerin gönderen ID'lerini topluyoruz
        var senderAccountIds = incomingTransactionsRaw.Select(x => x.AccountId).Distinct().ToList();

        // Bu ID'lere sahip hesapların Kullanıcı Bilgilerini çekiyoruz (JOIN işlemi)
        var senderInfo = await context.Accounts
            .AsNoTracking()
            .Include(x => x.User) // User tablosuna bağlan
            .Where(x => senderAccountIds.Contains(x.Id))
            .ToDictionaryAsync(k => k.Id, v => $"{v.User!.FirstName} {v.User.LastName}", cancellationToken);

        var incomingTransactions = incomingTransactionsRaw.Select(x => new AccountTransactionDto(
            x.Amount,
            x.Description ?? "Transfer",
            x.TransactionDate,
            x.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"),
            "Gelen Para",
            senderInfo.TryGetValue(x.AccountId, out var name) ? name : $"Gönderen ID: {x.AccountId}"
        )).ToList();

        // 4. Listeleri Birleştir ve Sırala (Yeniden eskiye)
        var allTransactions = outgoingTransactions
            .Concat(incomingTransactions)
            .OrderByDescending(x => x.Date)
            .ToList();

        // 5. Sonuç
        return new AccountDetailDto(
            account.Id,
            account.Iban,
            account.Balance,
            account.CurrencyType ==" 1" ? "TRY" : "USD", 
            allTransactions
        );
    }
}