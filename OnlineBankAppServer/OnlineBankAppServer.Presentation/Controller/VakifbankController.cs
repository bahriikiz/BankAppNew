using MediatR;
using Microsoft.AspNetCore.Mvc;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBankList;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetBranches;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetCities;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetDistricts;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNearest;
using OnlineBankAppServer.Application.Features.Vakifbank.Queries.GetNeighborhoods;
using OnlineBankAppServer.Presentation.Abstraction;

namespace OnlineBankAppServer.Presentation.Controller;

[Route("api/[controller]")]
public sealed class VakifbankController(IMediator mediator) : ApiController(mediator)
{
    // GET: api/vakifbank/cities
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetCitiesQuery(), cancellationToken);
        return Ok(response);
    }

    // GET: api/vakifbank/cities/{cityCode}/districts
    [HttpGet("cities/{cityCode}/districts")]
    public async Task<IActionResult> GetDistricts(string cityCode, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetDistrictsQuery(cityCode), cancellationToken);
        return Ok(response);
    }

    // GET: api/vakifbank/districts/{districtCode}/neighborhoods
    [HttpGet("districts/{districtCode}/neighborhoods")]
    public async Task<IActionResult> GetNeighborhoods(string districtCode, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetNeighborhoodsQuery(districtCode), cancellationToken);
        return Ok(response);
    }

    // GET: api/vakifbank/cities/{cityCode}/districts/{bankDistrictCode}/branches
    [HttpGet("cities/{cityCode}/districts/{bankDistrictCode}/branches")]
    public async Task<IActionResult> GetBranches(string cityCode, string bankDistrictCode, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetBranchesQuery(cityCode, bankDistrictCode), cancellationToken);
        return Ok(response);
    }

    // GET: api/vakifbank/banks
    [HttpGet("banks")]
    public async Task<IActionResult> GetBankList(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetBankListQuery(), cancellationToken);
        return Ok(response);
    }

    // GET: api/vakifbank/nearest?latitude={latitude}&longitude={longitude}&distanceLimit={distanceLimit}
    [HttpGet("nearest")]
    public async Task<IActionResult> GetNearest([FromQuery] string latitude, [FromQuery] string longitude, [FromQuery] int distanceLimit = 1, CancellationToken cancellationToken = default)
    {
        var response = await _mediator.Send(new GetNearestQuery(latitude, longitude, distanceLimit), cancellationToken);
        return Ok(response);
    }
}