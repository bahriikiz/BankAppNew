using MediatR;

namespace OnlineBankAppServer.Application.Features.Auth.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string PhoneNumber,
    string Address
) : IRequest<string>
{
   
}
