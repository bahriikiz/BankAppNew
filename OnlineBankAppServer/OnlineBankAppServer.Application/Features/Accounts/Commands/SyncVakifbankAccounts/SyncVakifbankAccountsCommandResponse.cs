namespace OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;

public sealed record SyncVakifbankAccountsCommandResponse(
    bool IsSuccess,
    string Message
);