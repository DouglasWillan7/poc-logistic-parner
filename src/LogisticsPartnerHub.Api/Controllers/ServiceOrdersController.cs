using LogisticsPartnerHub.Application.Commands.ServiceOrders;
using LogisticsPartnerHub.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsPartnerHub.Api.Controllers;

[ApiController]
[Route("api/service-orders")]
public class ServiceOrdersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return AcceptedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetServiceOrderQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetServiceOrdersQuery(), cancellationToken);
        return Ok(result);
    }
}
