using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNearest;

public sealed record GetNearestQuery(string Latitude, string Longitude, int DistanceLimit) : IRequest<VakifbankNearestResponseDto?>;