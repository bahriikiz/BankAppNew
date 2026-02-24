using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.SyncVakifbankAccounts;

// Kullanıcının ID'sini alıp, işlemin sonucunu (Response) dön
public sealed record SyncVakifbankAccountsCommand(int UserId) : IRequest<SyncVakifbankAccountsCommandResponse>;