using LogisticsPartnerHub.Application.Commands.PartnerEndpoints;
using LogisticsPartnerHub.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsPartnerHub.Api.Controllers;

[ApiController]
[Route("api/partners/{partnerId:guid}/endpoints")]
public class PartnerEndpointsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid partnerId, [FromBody] CreatePartnerEndpointCommand command, CancellationToken cancellationToken)
    {
        if (partnerId != command.PartnerId)
            return BadRequest("Route partnerId does not match command partnerId");

        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/partners/{partnerId}/endpoints/{result.Id}", result);
    }

    [HttpPut("{endpointId:guid}")]
    public async Task<IActionResult> Update(Guid partnerId, Guid endpointId, [FromBody] UpdatePartnerEndpointCommand command, CancellationToken cancellationToken)
    {
        if (endpointId != command.Id)
            return BadRequest("Route endpointId does not match command id");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid partnerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPartnerEndpointsQuery(partnerId), cancellationToken);
        return Ok(result);
    }
}
