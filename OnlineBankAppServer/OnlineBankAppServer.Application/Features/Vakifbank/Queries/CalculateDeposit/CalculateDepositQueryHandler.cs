using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.CalculateDeposit;

public sealed class CalculateDepositQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<CalculateDepositQuery, VakifbankDepositResponseDto?>
{
    public async Task<VakifbankDepositResponseDto?> Handle(CalculateDepositQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.CalculateDepositAsync(request.Amount, request.CurrencyCode, request.DepositType, request.CampaignId, request.TermDays, cancellationToken);
    }
}