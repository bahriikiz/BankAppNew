namespace OnlineBankAppServer.Application.Features.Auth.Commands.Login;
public sealed record LoginCommandResponse(
    string Token,
    string FirstName,
    string LastName,
    string Message = "");