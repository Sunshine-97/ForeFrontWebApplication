using ForeFrontWebApplication.Models.Order;
using MediatR;

namespace ForeFrontWebApplication.Commands;

public sealed record UpdateOrderStatusCommand(string OrderId, OrderStatus Status)
    : IRequest<Orders?>;
