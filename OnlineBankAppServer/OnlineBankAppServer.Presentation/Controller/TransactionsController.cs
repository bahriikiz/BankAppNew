using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Transactions.Commands.MoneyTransfer;
using OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Authorize]
public sealed class TransactionsController : ApiController
{
    public TransactionsController(IMediator mediator) : base(mediator) { }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(MoneyTransferCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }
    [HttpPost("get-activities")]
    public async Task<IActionResult> GetActivities(GetAccountActivitiesQuery request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}