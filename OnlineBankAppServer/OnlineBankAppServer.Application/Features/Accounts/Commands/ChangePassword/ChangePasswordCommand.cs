using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword
        ) : IRequest<string>;
    
}
