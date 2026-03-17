using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetDistricts;

public sealed record GetDistrictsQuery(string CityCode) : IRequest<VakifbankDistrictResponseDto?>;