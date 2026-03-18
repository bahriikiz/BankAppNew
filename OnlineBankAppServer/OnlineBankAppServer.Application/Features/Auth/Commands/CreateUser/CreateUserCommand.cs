using MediatR;
namespace OnlineBankAppServer.Application.Features.Auth.Commands.CreateUser
{
    public sealed record CreateUserCommand(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string IdentityNumber,
        string PhoneNumber,
        string City,
        string District,
        string Neighborhood,
        string Adress
        ) : IRequest<string>
    {
        public string Address { get; internal set; }
    }
}
