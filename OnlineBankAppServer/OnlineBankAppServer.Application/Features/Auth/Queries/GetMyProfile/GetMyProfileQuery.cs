using MediatR;

namespace OnlineBankAppServer.Application.Features.Auth.Queries.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<GetMyProfileResponse>;

public sealed record GetMyProfileResponse(
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber,
    string PhoneNumber,
    string City,
    string District,
    string Neighborhood,
    string Address,
    DateTime CreatedAt
);