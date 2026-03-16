using LogisticsPartnerHub.Application.Commands.Partners;
using LogisticsPartnerHub.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsPartnerHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartnerCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartnerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match command id");

        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPartnersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPartnerQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
