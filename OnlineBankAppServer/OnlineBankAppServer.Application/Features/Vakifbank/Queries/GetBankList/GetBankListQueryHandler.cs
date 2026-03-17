using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBankList;

public sealed class GetBankListQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetBankListQuery, VakifbankBankListResponseDto?>
{
    public async Task<VakifbankBankListResponseDto?> Handle(GetBankListQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetBankListAsync(cancellationToken);
    }
}