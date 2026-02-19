using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Beneficiaries.Commands.CreateBeneficiary;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
public sealed class BeneficiariesController : ApiController
{
    public BeneficiariesController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBeneficiaryCommand request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(new { Message = response });
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new OnlineBankAppServer.Application.Features.Beneficiaries.Queries.GetAllBeneficiaries.GetAllBeneficiariesQuery(), cancellationToken);
        return Ok(response);
    }
}