using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQuery(string DistrictCode) : IRequest<VakifbankNeighborhoodResponseDto?>;