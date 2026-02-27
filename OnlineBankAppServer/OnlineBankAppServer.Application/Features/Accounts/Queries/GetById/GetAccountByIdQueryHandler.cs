using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.DTOs;
using OnlineBankAppServer.Application.Integration.Vakifbank;
using OnlineBankAppServer.Persistance;
using System.Globalization;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetById;

internal sealed class GetAccountByIdQueryHandler(
    AppDbContext context,
    IVakifbankService vakifbankService) : IRequestHandler<GetAccountByIdQuery, AccountDetailDto>
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

        var allTransactions = new List<AccountTransactionDto>();

        // 2. EĞER HESAP VAKIFBANK HESABIYSA (CANLI VERİ ÇEK)
        if (!string.IsNullOrEmpty(account.RizaNo))
        {
            string cleanIban = account.Iban.Replace(" ", "");
            string accountNumber = cleanIban.Length >= 17 ? cleanIban.Substring(cleanIban.Length - 17) : cleanIban;

            // Varsayılan değerler
            decimal liveBalance = account.Balance;
            string liveCurrency = account.CurrencyType ?? "TRY";

            // --- A) CANLI HESAP BAKİYESİNİ ÇEK ---
            var detailResponse = await vakifbankService.GetAccountDetailAsync(account.RizaNo, accountNumber, cancellationToken);

            if (detailResponse?.Data?.AccountInfo != null)
            {
                decimal.TryParse(detailResponse.Data.AccountInfo.Balance?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out liveBalance);
                liveCurrency = detailResponse.Data.AccountInfo.CurrencyCode == "TL" ? "TRY" : (detailResponse.Data.AccountInfo.CurrencyCode ?? "TRY");
            }

            // --- B) SON 1 AYLIK HESAP HAREKETLERİNİ ÇEK ---
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddMonths(-1);

            var txResponse = await vakifbankService.GetAccountTransactionsAsync(
                account.RizaNo,
                accountNumber,
                startDate,
                endDate,
                cancellationToken);

            if (txResponse?.Data?.AccountTransactions != null)
            {
                foreach (var t in txResponse.Data.AccountTransactions)
                {
                    decimal.TryParse(t.Amount?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out decimal amount);
                    string type = t.TransactionType == "1" ? "Gelen Para" : "Giden Para";
                    DateTime.TryParse(t.TransactionDate, out DateTime transactionDate);

                    allTransactions.Add(new AccountTransactionDto(
                        amount,
                        t.Description ?? "VakıfBank İşlemi",
                        transactionDate,
                        transactionDate.ToString("dd.MM.yyyy HH:mm"),
                        type,
                        "Açık Bankacılık"
                    ));
                }
            }

            allTransactions = allTransactions.OrderByDescending(x => x.Date).ToList();

            // CANLI BAKİYE VE LİSTE İLE DÖNÜŞ YAP
            return new AccountDetailDto(
                account.Id,
                account.Iban,
                liveBalance,
                liveCurrency,
                allTransactions
            );
        }

        // 3. EĞER LOKAL HESAP İSE KENDİ VERİTABANIMIZDAN ÇEK (Eski Mantık)

        // Giden Paralar (Benim gönderdiklerim)
        var outgoingTransactions = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.AccountId == account.Id)
            .Select(x => new AccountTransactionDto(
                x.Amount,
                x.Description ?? "Transfer",
                x.TransactionDate,
                x.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"),
                "Giden Para",
                x.TargetIban ?? "Bilinmiyor"
            ))
            .ToListAsync(cancellationToken);

        // Gelen Paralar (Bana gelenler)
        var incomingTransactionsRaw = await context.BankTransactions
            .AsNoTracking()
            .Where(x => x.TargetIban == account.Iban)
            .ToListAsync(cancellationToken);

        var senderAccountIds = incomingTransactionsRaw.Select(x => x.AccountId).Distinct().ToList();

        var senderInfo = await context.Accounts
            .AsNoTracking()
            .Include(x => x.User)
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
        allTransactions = outgoingTransactions
            .Concat(incomingTransactions)
            .OrderByDescending(x => x.Date)
            .ToList();

        // 5. Sonuç (Lokal hesap bakiyesiyle dönüş)
        return new AccountDetailDto(
            account.Id,
            account.Iban,
            account.Balance,
            account.CurrencyType ?? "TRY",
            allTransactions
        );
    }
}