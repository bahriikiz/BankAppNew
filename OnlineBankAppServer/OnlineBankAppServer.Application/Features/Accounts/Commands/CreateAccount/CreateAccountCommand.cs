using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.CreateAccount
{
    public sealed record CreateAccountCommand(
        string AccountName,
        string CurrencyType) : IRequest<string>;
}