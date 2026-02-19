using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;
namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetByUserId
{
    internal sealed class GetAccountByUserIdQueryHandler(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetAccountByUserIdQuery, List<Account>>
    {
        public async Task<List<Account>> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı. Lütfen giriş yapınız.");
            
            int userId = int.Parse(userIdClaim.Value);

            var accounts = await context.Accounts
                .Where(x => x.UserId == userId)
                .Include(x => x.Bank)
                .ToListAsync(cancellationToken);
            return accounts;
        }
    }
}
