using MediatR;
using OnlineBankAppServer.Domain.Entities;
namespace OnlineBankAppServer.Application.Features.Accounts.Queries.GetByUserId
{
    public sealed record GetAccountByUserIdQuery( ) : IRequest<List<Account>>;
    
}
