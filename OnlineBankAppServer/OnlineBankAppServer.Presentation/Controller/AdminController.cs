using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Admin.Queries.GetDashboard;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Authorize(Roles = "Admin")]
public sealed class AdminController : ApiController
{
    public AdminController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);
        return Ok(response);
    }

}