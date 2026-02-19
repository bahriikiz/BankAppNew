using MediatR;
namespace OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser
{
    public sealed record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password) : IRequest<string>;


}
