using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.DTOs;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Admin.Queries.GetDashboard;

internal sealed class GetAdminDashboardQueryHandler(
    AppDbContext context) : IRequestHandler<GetAdminDashboardQuery, AdminDashboardDto>
{
    public async Task<AdminDashboardDto> Handle(GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        // 1. Toplam Kullanıcı ve Hesap Sayısı
        var totalUsers = await context.Users.CountAsync(cancellationToken);
        var totalAccounts = await context.Accounts.CountAsync(cancellationToken);

        // 2. Para Birimine Göre Bankadaki Toplam Paralar (TRY, USD vs.)
        var totalBalances = await context.Accounts
            .AsNoTracking()
            .GroupBy(a => a.CurrencyType)
            .Select(g => new CurrencyTotalDto(
                g.Key ==" 1" ? "TRY" : "USD",
                g.Sum(a => a.Balance)
            ))
            .ToListAsync(cancellationToken);

        // 3. Bankadaki Son 10 İşlem (Tüm kullanıcıların işlemleri)
        var recentTransactionsRaw = await context.BankTransactions
            .AsNoTracking()
            .OrderByDescending(t => t.TransactionDate)
            .Take(10) // Sadece son 10 işlem
            .ToListAsync(cancellationToken);

        // Gönderenlerin isimlerini bulmak için JOIN mantığı
        var accountIds = recentTransactionsRaw.Select(x => x.AccountId).Distinct().ToList();

        var senderInfo = await context.Accounts
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => accountIds.Contains(x.Id))
            .ToDictionaryAsync(k => k.Id, v => $"{v.User!.FirstName} {v.User.LastName}", cancellationToken);

        // Listeyi DTO'ya çevir
        var recentTransactions = recentTransactionsRaw.Select(t => new AdminTransactionDto(
            senderInfo.TryGetValue(t.AccountId, out var name) ? name : $"Hesap ID: {t.AccountId}", // Gönderen
            t.TargetIban ?? "Bilinmiyor",                                                          // Alıcı
            t.Amount,                                                                              // Tutar
            t.TransactionDate.AddHours(3).ToString("dd.MM.yyyy HH:mm"),                            // TR Saati
            t.Description ?? "Transfer"                                                            // Açıklama
        )).ToList();

        // 4. Sonuçları DTO'ya Doldur ve Gönder
        return new AdminDashboardDto(
            totalUsers,
            totalAccounts,
            totalBalances,
            recentTransactions
        );
    }
}