using LogisticsPartnerHub.Application.Commands.FieldMappings;
using LogisticsPartnerHub.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsPartnerHub.Api.Controllers;

[ApiController]
[Route("api/partners/{partnerId:guid}/field-mappings")]
public class FieldMappingsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid partnerId, [FromBody] CreateFieldMappingCommand command, CancellationToken cancellationToken)
    {
        if (partnerId != command.PartnerId)
            return BadRequest("Route partnerId does not match command partnerId");

        var result = await mediator.Send(command, cancellationToken);
        return Created($"/api/partners/{partnerId}/field-mappings/{result.Id}", result);
    }

    [HttpPut("{mappingId:guid}")]
    public async Task<IActionResult> Update(Guid partnerId, Guid mappingId, [FromBody] UpdateFieldMappingCommand command, CancellationToken cancellationToken)
    {
        if (mappingId != command.Id)
            return BadRequest("Route mappingId does not match command id");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid partnerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFieldMappingsQuery(partnerId), cancellationToken);
        return Ok(result);
    }
}
