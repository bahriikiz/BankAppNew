using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetDistricts;

public sealed class GetDistrictsQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetDistrictsQuery, VakifbankDistrictResponseDto?>
{
    public async Task<VakifbankDistrictResponseDto?> Handle(GetDistrictsQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetDistrictsAsync(request.CityCode, cancellationToken);
    }
}