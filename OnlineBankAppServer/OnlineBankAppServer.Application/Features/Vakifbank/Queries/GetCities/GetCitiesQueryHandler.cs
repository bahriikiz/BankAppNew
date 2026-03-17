using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetCities;

public sealed class GetCitiesQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetCitiesQuery, VakifbankCityResponseDto?>
{
    public async Task<VakifbankCityResponseDto?> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetCitiesAsync(cancellationToken);
    }
}