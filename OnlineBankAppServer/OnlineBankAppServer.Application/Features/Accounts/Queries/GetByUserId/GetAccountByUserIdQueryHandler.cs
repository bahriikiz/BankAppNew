using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Application.Integration.Vakifbank;
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
    public async Task<List<Account>> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı. Lütfen giriş yapınız.");

        int userId = int.Parse(userIdClaim.Value);

        // Hesapları veritabanından çek (AsNoTracking ile daha hızlı)
        var accounts = await context.Accounts
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Bank)
            .ToListAsync(cancellationToken);

        // YENİ EKLENEN KISIM: Her bir hesap için döngüye girip VakıfBank hesabıysa canlı bakiye çekiyoruz!
        foreach (var account in accounts)
        {
            if (!string.IsNullOrEmpty(account.RizaNo))
            {
                try
                {
                    string cleanIban = account.Iban.Replace(" ", "");
                    string accountNumber = cleanIban.Length >= 17 ? cleanIban.Substring(cleanIban.Length - 17) : cleanIban;

                    // VakıfBank'tan canlı hesap detaylarını çekiyoruz
                    var detailResponse = await vakifbankService.GetAccountDetailAsync(account.RizaNo, accountNumber, cancellationToken);

                    if (detailResponse?.Data?.AccountInfo != null)
                    {
                        // API'den gelen canlı bakiyeyi decimal'a çevirip mevcut yerel bakiyenin üzerine yazıyoruz
                        if (decimal.TryParse(detailResponse.Data.AccountInfo.Balance?.Replace(".", ","), NumberStyles.Any, new CultureInfo("tr-TR"), out decimal liveBalance))
                        {
                            account.Balance = liveBalance;
                        }

                        // Arayüzde VakıfBank'tan gelen "TL" yerine modern "TRY" görünmesi için düzeltme
                        account.CurrencyType = detailResponse.Data.AccountInfo.CurrencyCode == "TL" ? "TRY" : (detailResponse.Data.AccountInfo.CurrencyCode ?? account.CurrencyType);
                    }
                }
                catch
                {
                
                }
            }
        }

        return accounts;
    }
}