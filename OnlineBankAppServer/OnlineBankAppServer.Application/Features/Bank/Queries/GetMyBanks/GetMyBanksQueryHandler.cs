using MediatR;
using Microsoft.AspNetCore.Http; 
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Banks.Queries.GetMyBanks;

internal sealed class GetMyBanksQueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetMyBanksQuery, List<Bank>>
{
    public async Task<List<Bank>> Handle(GetMyBanksQuery request, CancellationToken cancellationToken)
    {
        // Giriş yapan kullanıcının ID'sini bul
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // Sadece kullanıcının hesabı olan bankaları getir
        // Banks tablosuna git -> Accounts listesine bak -> UserId benimkiyle eşleşen var mı?
        var myBanks = await context.Banks
              .Include(b => b.Accounts) 
              .Where(b => b.Accounts != null && b.Accounts.Any(a => a.UserId == userId))
              .ToListAsync(cancellationToken);

        return myBanks;
    }
}