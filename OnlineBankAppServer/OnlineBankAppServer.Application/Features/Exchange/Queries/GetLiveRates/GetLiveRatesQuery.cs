using MediatR;
using OnlineBankAppServer.Application.DTOs;

namespace OnlineBankAppServer.Application.Features.Exchange.Queries.GetLiveRates;

public sealed record GetLiveRatesQuery() : IRequest<LiveExchangeRatesDto>;