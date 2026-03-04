using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Auth.Queries.GetMyProfile;

internal sealed class GetMyProfileQueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetMyProfileQuery, GetMyProfileResponse>
{
    public async Task<GetMyProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub");
        if (userIdClaim is null) throw new Exception("Kimlik dogrulanamadı.");

        int userId = int.Parse(userIdClaim.Value);

        var user =await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null) throw new Exception("Kullanıcı bulunamadı.");

        return new GetMyProfileResponse(
            user.FirstName,
            user.LastName,
            user.Email,
            user.IdentityNumber,
            user.PhoneNumber,
            user.Adress,
            user.CreatedAt
            );
    }
    
}