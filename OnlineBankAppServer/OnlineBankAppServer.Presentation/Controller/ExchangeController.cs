using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Exchange.Queries.GetLiveRates;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Authorize]
public sealed class ExchangeController : ApiController
{
    public ExchangeController(IMediator mediator) : base(mediator)
    {
    }
    [AllowAnonymous]
    [HttpGet("live-rates")]
    public async Task<IActionResult> GetLiveRates(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetLiveRatesQuery(), cancellationToken);
        return Ok(response);
    }
}