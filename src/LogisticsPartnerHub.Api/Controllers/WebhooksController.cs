using LogisticsPartnerHub.Application.Commands.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogisticsPartnerHub.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(IMediator mediator) : ControllerBase
{
    [HttpPost("{partnerId:guid}")]
    public async Task<IActionResult> HandleWebhook(Guid partnerId, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        var result = await mediator.Send(new HandleWebhookCommand(partnerId, payload), cancellationToken);
        return result ? Ok() : BadRequest("Failed to process webhook");
    }
}
