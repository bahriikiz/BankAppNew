using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;

// Dışardan rıza no alacak
public sealed record SyncVakifbankAccountsCommand(
    string RizaNo
) : IRequest<SyncVakifbankAccountsCommandResponse>
{
    public int UserId { get; set; }
}