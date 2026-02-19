using MediatR;
using Microsoft.EntityFrameworkCore;
using OnlineBankAppServer.Domain.Entities;
using OnlineBankAppServer.Persistance;

namespace OnlineBankAppServer.Application.Features.Banks.Queries.GetAllBanks;

internal sealed class GetAllBanksQueryHandler(
    AppDbContext context) : IRequestHandler<GetAllBanksQuery, List<Bank>>
{
    public async Task<List<Bank>> Handle(GetAllBanksQuery request, CancellationToken cancellationToken)
    {
        // Tüm bankaları getir (Ziraat, Vakıf, Garanti...)
        return await context.Banks.ToListAsync(cancellationToken);
    }
}