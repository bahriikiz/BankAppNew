using MediatR;
using OnlineBankAppServer.Application.Abstractions;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBranches;

public sealed class GetBranchesQueryHandler(IVakifbankService vakifbankService)
    : IRequestHandler<GetBranchesQuery, VakifbankBranchResponseDto?>
{
    public async Task<VakifbankBranchResponseDto?> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        return await vakifbankService.GetBranchesAsync(request.CityCode, request.BankDistrictCode, cancellationToken);
    }
}