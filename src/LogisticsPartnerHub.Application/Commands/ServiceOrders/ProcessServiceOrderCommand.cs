using MediatR;

namespace LogisticsPartnerHub.Application.Commands.ServiceOrders;

public record ProcessServiceOrderCommand(Guid ServiceOrderId) : IRequest<bool>;
