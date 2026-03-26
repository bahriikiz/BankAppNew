using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Globalization;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;

internal sealed class GetAccountActivitiesQueryHandler(
    AppDbContext context,
    IVakifbankService vakifbankService) : IRequestHandler<GetAccountActivitiesQuery, List<BankTransaction>>
{
    // Performans için kültürü bir kere tanımlıyoruz
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public async Task<List<BankTransaction>> Handle(GetAccountActivitiesQuery request, CancellationToken cancellationToken)
    {
        // 1. Hesabı Bul (Genel Exception yerine KeyNotFoundException)
        var account = await context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException("Hesap bulunamadı!");

        // 2. Tarih Filtrelerini Belirle
        (DateTime startDate, DateTime endDate) = DetermineDateRange(request.StartDate, request.EndDate);

        // 3. Hesap Türüne Göre Veriyi Çek (Açık Bankacılık vs Lokal)
        if (!string.IsNullOrEmpty(account.RizaNo) && account.RizaNo != null)
        {
            return await GetVakifbankTransactionsAsync(account, startDate, endDate, cancellationToken);
        }

        return await GetLocalTransactionsAsync(request.AccountId, startDate, endDate, cancellationToken);
    }

    // --- YARDIMCI (PRIVATE) METODLAR ---

    private static (DateTime StartDate, DateTime EndDate) DetermineDateRange(DateTime? reqStartDate, DateTime? reqEndDate)
    {
        DateTime endDate = reqEndDate ?? DateTime.Now;
        DateTime startDate = reqStartDate ?? endDate.AddMonths(-1);

        // VakıfBank "ACBH000134" hatası vermesin diye tarih aralığını maksimum 364 günle sınırlıyoruz.
        if ((endDate - startDate).TotalDays > 365)
        {
            startDate = endDate.AddDays(-364);
        }

        return (startDate, endDate);
    }

    private async Task<List<BankTransaction>> GetVakifbankTransactionsAsync(Account account, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        string cleanIban = account.Iban.Replace(" ", "");
        string accountNumber = cleanIban.Length >= 17 ? cleanIban[^17..] : cleanIban;

        var apiResponse = await vakifbankService.GetAccountTransactionsAsync(
            account.RizaNo!,
            accountNumber,
            startDate,
            endDate,
            cancellationToken);

        var apiTransactions = new List<BankTransaction>();
        var fetchedTransactions = apiResponse?.Data?.AccountTransactions;

        if (fetchedTransactions == null)
            return apiTransactions;

        foreach (var t in fetchedTransactions)
        {
            decimal.TryParse(t.Amount?.Replace(".", ","), NumberStyles.Any, TrCulture, out decimal amount);

            if (t.TransactionType == "2" && amount > 0)
                amount = -amount;

            DateTime transactionDate = ParseTransactionDate(t.TransactionDate);

            apiTransactions.Add(new BankTransaction
            {
                AccountId = account.Id,
                Amount = amount,
                Description = t.Description ?? "Açıklama Yok",
                TransactionDate = transactionDate,
                CreatedDate = transactionDate,
                TransactionReference = t.TransactionId
            });
        }

        return [.. apiTransactions.OrderByDescending(x => x.TransactionDate)];
    }

    private static DateTime ParseTransactionDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return DateTime.Now;

        string[] formats =
        [
            "dd.MM.yyyy HH:mm:ss",    // 25.12.2024 14:30:45
            "dd.MM.yyyy",              // 25.12.2024
            "dd-MM-yyyy",              // 25-12-2024
            "yyyy-MM-dd",              // 2024-12-25
            "yyyy-MM-dd HH:mm:ss",     // 2024-12-25 14:30:45
            "MM/dd/yyyy",              // 12/25/2024
            "dd/MM/yyyy"               // 25/12/2024
        ];

        // Türkçe format provider ile ayrıştırma dene
        if (DateTime.TryParseExact(dateString, formats, TrCulture, DateTimeStyles.None, out DateTime trDate))
            return trDate;

        // İngilizce (ABD) format provider ile dene
        var enUsCulture = CultureInfo.GetCultureInfo("en-US");
        if (DateTime.TryParseExact(dateString, formats, enUsCulture, DateTimeStyles.None, out DateTime enDate))
            return enDate;

        // Son çare: genel TryParse
        if (DateTime.TryParse(dateString, TrCulture, DateTimeStyles.None, out DateTime result))
            return result;

        // Hata durumunda günümüzün tarihi dön
        return DateTime.Now;
    }

    private async Task<List<BankTransaction>> GetLocalTransactionsAsync(int accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await context.BankTransactions
            .AsNoTracking()
            .Where(p => p.AccountId == accountId && p.TransactionDate >= startDate && p.TransactionDate <= endDate)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync(cancellationToken);
    }
}