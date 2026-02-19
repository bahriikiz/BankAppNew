using MediatR;
namespace OnlineBankAppServer.Application.Features.Accounts.Commands.CreateAccount
{
    public sealed record CreateAccountCommand(
        string CurrencyType,
        int BankId) : IRequest<string>;

}
