using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.AI.Commands.AskAI;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Route("api/[controller]")]
public sealed class AiController(IMediator mediator) : ApiController(mediator)
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskAICommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }
}