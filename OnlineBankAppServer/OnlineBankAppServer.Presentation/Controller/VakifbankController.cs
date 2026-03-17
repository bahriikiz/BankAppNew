using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetCities;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Route("api/[controller]")]
public sealed class VakifbankController(IMediator mediator) : ApiController(mediator)
{
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCitiesQuery(), cancellationToken);
        return Ok(response);
    }
}