using MediatR;
using OnlineBankAppServer.Domain.Abstractions;

namespace OnlineBankAppServer.Application.Features.Auth.Commands.DeleteUser;

public sealed record DeleteUserCommand(
    int UserId
) : IRequest<Result<string>>;