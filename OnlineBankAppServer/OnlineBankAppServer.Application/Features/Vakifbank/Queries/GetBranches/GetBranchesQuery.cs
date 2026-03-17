using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBranches;

public sealed record GetBranchesQuery(string CityCode, string BankDistrictCode) : IRequest<VakifbankBranchResponseDto?>;