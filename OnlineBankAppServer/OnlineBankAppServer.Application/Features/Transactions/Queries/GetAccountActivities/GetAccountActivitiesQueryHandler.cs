using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Integration.Vakifbank;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Globalization;

namespace OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;

internal sealed class GetAccountActivitiesQueryHandler(
    AppDbContext context,
    IVakifbankService vakifbankService) : IRequestHandler<GetAccountActivitiesQuery, List<BankTransaction>>
{
    public async Task<List<BankTransaction>> Handle(GetAccountActivitiesQuery request, CancellationToken cancellationToken)
    {
        // 1. Hesabı bul
        var account = await context.Accounts
            .AsNoTracking() // Sadece okuma yaptığımız için performansı artırır
            .FirstOrDefaultAsync(p => p.Id == request.AccountId, cancellationToken);

        if (account is null)
            throw new Exception("Hesap bulunamadı!");

        // 2. TARİH FİLTRELERİ (Eğer UI'dan tarih gelmezse varsayılan olarak son 1 ayı getiririz)
        DateTime endDate = request.EndDate ?? DateTime.Now;
        DateTime startDate = request.StartDate ?? endDate.AddMonths(-1);

        // VakıfBank "ACBH000134" hatası vermesin diye tarih aralığını maksimum 364 günle sınırlıyoruz.
        if ((endDate - startDate).TotalDays > 365)
        {
            startDate = endDate.AddDays(-364);
        }

        if (!string.IsNullOrEmpty(account.RizaNo))
        {
            // IBAN'daki boşlukları temizle ve son 17 haneyi al (VakıfBank 17 hane ister)
            string cleanIban = account.Iban.Replace(" ", "");
            string accountNumber = cleanIban.Length >= 17 ? cleanIban.Substring(cleanIban.Length - 17) : cleanIban;

            // API'den verileri getir
            var apiResponse = await vakifbankService.GetAccountTransactionsAsync(
                account.RizaNo,
                accountNumber,
                startDate,
                endDate,
                cancellationToken);

            var apiTransactions = new List<BankTransaction>();

            if (apiResponse?.Data?.AccountTransactions != null)
            {
                foreach (var t in apiResponse.Data.AccountTransactions)
                {
                    // Tutarı ondalıklı olarak string'den decimal'a çevir
                    decimal.TryParse(t.Amount?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out decimal amount);

                    // API'den gelen miktar bazen eksi (-) olmayabilir, TransactionType "2" ise eksiye çevir
                    if (t.TransactionType == "2" && amount > 0)
                        amount = -amount;

                    // Tarihi parse et
                    DateTime.TryParse(t.TransactionDate, out DateTime transactionDate);

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
            }

            // Tarihe göre en yeniden eskiye doğru sıralayıp frontend'e gönderiyoruz
            return apiTransactions.OrderByDescending(x => x.TransactionDate).ToList();
        }

        // 4. EĞER BİZİM LOKAL BANKAMIZIN (TEST) HESABIYSA KENDİ DB'MİZDEN OKU
        var localTransactions = await context.BankTransactions
            .AsNoTracking()
            // Hem hesaba hem de girilen tarih aralığına göre filtrele
            .Where(p => p.AccountId == request.AccountId && p.TransactionDate >= startDate && p.TransactionDate <= endDate)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync(cancellationToken);

        return localTransactions;
    }
}