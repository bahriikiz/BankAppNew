using MediatR;

namespace OnlineBankAppServer.Application.Features.AI.Commands.AskAI;

public sealed record AskAICommand(
    string Message
) : IRequest<string>;