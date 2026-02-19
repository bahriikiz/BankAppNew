using MediatR;
using OnlineBankAppServer.Application.DTOs;

namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetById;

public sealed record GetAccountByIdQuery(
    int AccountId,
    int UserId
) : IRequest<AccountDetailDto>;