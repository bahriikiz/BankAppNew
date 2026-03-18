using MediatR;

namespace OnlineBankAppServer.Application.Features.Accounts.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string PhoneNumber,
    string City,
    string District,
    string Neighborhood,
    string Address
) : IRequest<string>;