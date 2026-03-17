using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNeighborhoods;

public sealed class GetNeighborhoodsQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetNeighborhoodsQuery, VakifbankNeighborhoodResponseDto?>
{
    public async Task<VakifbankNeighborhoodResponseDto?> Handle(GetNeighborhoodsQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetNeighborhoodsAsync(request.DistrictCode, cancellationToken);
    }
}