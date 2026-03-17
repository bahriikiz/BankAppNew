using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetCities;

public sealed record GetCitiesQuery() : IRequest<VakifbankCityResponseDto?>;