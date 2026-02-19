using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Banks.Queries.GetAllBanks;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

public sealed class BanksController : ApiController
{
    public BanksController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAllBanksQuery(), cancellationToken);
        return Ok(response);
    }
    [HttpGet("my-banks")]
    public async Task<IActionResult> GetMyBanks(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new OnlineBankAppServer.Application.Features.Banks.Queries.GetMyBanks.GetMyBanksQuery(), cancellationToken);
        return Ok(response);
    }
}