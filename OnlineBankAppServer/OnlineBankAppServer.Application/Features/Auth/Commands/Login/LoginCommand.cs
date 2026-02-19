using MediatR;
namespace OnlineBankAppServer.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand(
        string Email,
        string Password) : IRequest<LoginCommandResponse>;
    
}
