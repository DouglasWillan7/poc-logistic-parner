using MediatR;

namespace LogisticsPartnerHub.Application.Commands.Webhooks;

public record HandleWebhookCommand(Guid PartnerId, string Payload) : IRequest<bool>;
