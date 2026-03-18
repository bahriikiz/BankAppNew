using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetDepositProducts;

public sealed class GetDepositProductsQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetDepositProductsQuery, VakifbankDepositProductResponseDto?>
{
    public async Task<VakifbankDepositProductResponseDto?> Handle(GetDepositProductsQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetDepositProductsAsync(cancellationToken);
    }
}