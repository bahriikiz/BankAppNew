using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Globalization;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetByUserId;

internal sealed class GetAccountByUserIdQueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IVakifbankService vakifbankService) : IRequestHandler<GetAccountByUserIdQuery, List<Account>>
{
    // Performans için kültürü sadece bir kere (static olarak) tanımlıyoruz
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public async Task<List<Account>> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = (httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)) ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı. Lütfen giriş yapınız.");
        int userId = int.Parse(userIdClaim.Value);

        // Hesapları veritabanından çek 
        var accounts = await context.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Bank)
            .ToListAsync(cancellationToken);

        // Canlı bakiyeleri güncelle 
        await EnrichWithLiveBalancesAsync(accounts, cancellationToken);

        return accounts;
    }

    private async Task EnrichWithLiveBalancesAsync(List<Account> accounts, CancellationToken cancellationToken)
    {
        // Rıza no olanları listele
        var openBankingAccounts = accounts.Where(a => !string.IsNullOrEmpty(a.RizaNo));

        foreach (var account in openBankingAccounts)
        {
            await UpdateLiveBalanceAsync(account, cancellationToken);
        }
    }

    private async Task UpdateLiveBalanceAsync(Account account, CancellationToken cancellationToken)
    {
        try
        {
            string cleanIban = account.Iban.Replace(" ", "");
            string accountNumber = cleanIban.Length >= 17 ? cleanIban[^17..] : cleanIban;

            // VakıfBank'tan canlı hesap detaylarını çekiyoruz
            var detailResponse = await vakifbankService.GetAccountDetailAsync(account.RizaNo!, accountNumber, cancellationToken);

            var accountInfo = detailResponse?.Data?.AccountInfo;
            if (accountInfo != null)
            {
                // API'den gelen canlı bakiyeyi decimal'a çevirip mevcut yerel bakiyenin üzerine yazıyoruz
                if (decimal.TryParse(accountInfo.Balance?.Replace(".", ","), NumberStyles.Any, TrCulture, out decimal liveBalance))
                {
                    account.Balance = liveBalance;
                }

                account.CurrencyType = accountInfo.CurrencyCode == "TL" ? "TRY" : (accountInfo.CurrencyCode ?? account.CurrencyType);
            }
        }
        catch
        {
            // Api çökerse veya ulaşılamazsa kendi veritabanımızdan döner.
        }
    }
}