using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.CalculateDeposit;

public sealed record CalculateDepositQuery(decimal Amount, string CurrencyCode, long DepositType, long CampaignId, int TermDays) : IRequest<VakifbankDepositResponseDto?>;