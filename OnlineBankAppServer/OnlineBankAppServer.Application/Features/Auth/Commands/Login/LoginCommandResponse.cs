namespace OnlineBankAppServer.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommandResponse(
        string? Token,
        string Message);

}
