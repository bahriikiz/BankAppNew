using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNearest;

public sealed class GetNearestQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetNearestQuery, VakifbankNearestResponseDto?>
{
    public async Task<VakifbankNearestResponseDto?> Handle(GetNearestQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetNearestBranchAndAtmAsync(request.Latitude, request.Longitude, request.DistanceLimit, cancellationToken);
    }
}