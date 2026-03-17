using MediatR;
using OnlineBankAppServer.Application.Integration.Vakifbank.Dtos;

namespace OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBankList;

public sealed record GetBankListQuery() : IRequest<VakifbankBankListResponseDto?>;