using MediatR;

namespace OnlineBankAppServer.Application.Features.Auth.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<GetMyProfileResponse>;

public sealed record GetMyProfileResponse
(
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    string PhoneNumber,
    string Address,
    DateTime CreatedAt
);