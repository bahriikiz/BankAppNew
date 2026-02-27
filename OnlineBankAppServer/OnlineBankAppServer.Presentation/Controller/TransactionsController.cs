using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Transactions.Commands.MoneyTransfer;
using OnlineBankAppServer.Application.Features.Transactions.Queries.GetAccountActivities;
using OnlineBankAppServer.Application.Features.Transactions.Queries.GetReceipt;
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

    // --- YENİ: DEKONT SORGULAMA ---
    [HttpGet("{accountId}/receipt/{transactionId}")]
    public async Task<IActionResult> GetReceipt(int accountId, string transactionId, [FromQuery] string format = "2", CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetTransactionReceiptQuery(accountId, transactionId, format), cancellationToken);
        return Ok(new { Data = response });
    }
}