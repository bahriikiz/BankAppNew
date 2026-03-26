using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.DTOs;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Globalization;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetById
{
    public sealed class GetAccountByIdQueryHandler(
        AppDbContext context,
        IVakifbankService vakifbankService) : IRequestHandler<GetAccountByIdQuery, AccountDetailDto>
    {
        private static readonly CultureInfo TrCulture = new("tr-TR");

        public async Task<AccountDetailDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Hesabı Bul (Spesifik Exception ile)
            var account = await context.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.AccountId && x.UserId == request.UserId, cancellationToken)
                ?? throw new KeyNotFoundException("Hesap bulunamadı veya bu işlem için yetkiniz yok!");

            // 2. Karar Mekanizması: Vakıfbank mı yoksa Lokal mi?
            if (!string.IsNullOrEmpty(account.RizaNo))
            {
                return await GetVakifbankAccountDetailAsync(account, cancellationToken);
            }

            return await GetLocalAccountDetailAsync(account, cancellationToken);
        }

        private async Task<AccountDetailDto> GetVakifbankAccountDetailAsync(Account account, CancellationToken cancellationToken)
        {
            string rizaNo = account.RizaNo!;
            string cleanIban = account.Iban.Replace(" ", "");
            string accountNumber = cleanIban.Length >= 17 ? cleanIban[^17..] : cleanIban;
            decimal liveBalance = account.Balance;
            string liveCurrency = account.CurrencyType ?? "TRY";
            var transactions = new List<AccountTransactionDto>();

            try
            {
                // A) Canlı Bakiye
                var detail = await vakifbankService.GetAccountDetailAsync(rizaNo, accountNumber, cancellationToken);
                if (detail?.Data?.AccountInfo != null)
                {
                    // Bakiye sıfırlanmasını önlüyoruz
                    if (decimal.TryParse(detail.Data.AccountInfo.Balance?.Replace(".", ","), NumberStyles.Any, TrCulture, out decimal parsedBalance))
                    {
                        liveBalance = parsedBalance;
                    }
                    liveCurrency = detail.Data.AccountInfo.CurrencyCode == "TL" ? "TRY" : detail.Data.AccountInfo.CurrencyCode ?? "TRY";
                }

                // B) Canlı Hareketler
                var txResponse = await vakifbankService.GetAccountTransactionsAsync(rizaNo, accountNumber, DateTime.Now.AddMonths(-1), DateTime.Now, cancellationToken);
                if (txResponse?.Data?.AccountTransactions != null)
                {
                    transactions = MapVakifbankTransactions(txResponse.Data.AccountTransactions);
                }
            }
            catch { /* API Hatasında lokal verilerle devam et */ }

            return new AccountDetailDto(account.Id, account.Iban, liveBalance, liveCurrency, [.. transactions.OrderByDescending(x => x.Date)]);
        }

        private static List<AccountTransactionDto> MapVakifbankTransactions(IEnumerable<dynamic> apiTransactions)
        {
            var list = new List<AccountTransactionDto>();

            foreach (var t in apiTransactions)
            {
                decimal.TryParse(t.Amount?.Replace(".", ","), NumberStyles.Any, TrCulture, out decimal amount);
                DateTime.TryParse(t.TransactionDate, out DateTime transactionDate);

                list.Add(new AccountTransactionDto(
                    amount,
                    t.Description ?? "VakıfBank İşlemi",
                    transactionDate,
                    transactionDate.ToString("dd.MM.yyyy HH:mm"),
                    t.TransactionType == "1" ? "Gelen Para" : "Giden Para",
                    "Açık Bankacılık",
                    t.TransactionId));
            }

            return list;
        }

        private async Task<AccountDetailDto> GetLocalAccountDetailAsync(Account account, CancellationToken cancellationToken)
        {
            // Giden Paralar
            var outgoing = await context.BankTransactions
                .AsNoTracking()
                .Where(x => x.AccountId == account.Id)
                .Select(x => new AccountTransactionDto(
                    x.Amount, x.Description ?? "Transfer", x.TransactionDate,
                    x.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"),
                    "Giden Para", x.TargetIban ?? "Bilinmiyor", null))
                .ToListAsync(cancellationToken);

            // Gelen Paralar ve Gönderen Bilgisi
            var incomingRaw = await context.BankTransactions
                .AsNoTracking()
                .Where(x => x.TargetIban == account.Iban)
                .ToListAsync(cancellationToken);

            var senderIds = incomingRaw.Select(x => x.AccountId).Distinct().ToList();
            var senderInfo = await context.Accounts
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => senderIds.Contains(x.Id))
                .ToDictionaryAsync(k => k.Id, v => $"{v.User!.FirstName} {v.User.LastName}", cancellationToken);

            var incoming = incomingRaw.Select(x => new AccountTransactionDto(
                x.Amount, x.Description ?? "Transfer", x.TransactionDate,
                x.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"),
                "Gelen Para",
                senderInfo.TryGetValue(x.AccountId, out var name) ? name : $"Gönderen ID: {x.AccountId}",
                null)).ToList();

            var allTransactions = outgoing.Concat(incoming).OrderByDescending(x => x.Date).ToList();
            return new AccountDetailDto(account.Id, account.Iban, account.Balance, account.CurrencyType ?? "TRY", allTransactions);
        }
    }
}