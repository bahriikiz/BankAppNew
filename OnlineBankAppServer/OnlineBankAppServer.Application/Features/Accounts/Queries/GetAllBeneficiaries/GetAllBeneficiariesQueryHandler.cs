using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;
using System.Security.Claims;

namespace OnlineBankAppServer.Application.Features.Beneficiaries.Queries.GetAllBeneficiaries;

internal sealed class GetAllBeneficiariesQueryHandler(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetAllBeneficiariesQuery, List<Beneficiary>>
{
    public async Task<List<Beneficiary>> Handle(GetAllBeneficiariesQuery request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı Bul
        var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null) throw new Exception("Kullanıcı bulunamadı.");
        int userId = int.Parse(userIdClaim.Value);

        // 2. Sadece bu kullanıcıya ait kayıtları getir
        return await context.Beneficiaries
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedDate) 
            .ToListAsync(cancellationToken);
    }
}