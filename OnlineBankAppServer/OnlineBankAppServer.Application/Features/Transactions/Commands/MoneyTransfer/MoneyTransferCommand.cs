using MediatR;
namespace OnlineBankAppServer.Application.Features.Transactions.Commands.MoneyTransfer
{
    public sealed record MoneyTransferCommand(
        int AccountId,
        decimal Amount,
        string Description,
        string? TargetIban,
        int? BeneficiaryId
) : IRequest<string>;

}
