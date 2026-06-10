using ForeFrontWebApplication.DTOs.Order;
using MediatR;

namespace ForeFrontWebApplication.Commands;

public sealed record CreateOrderCommand(string KundId, List<OrderItemRequest> Produkter) : IRequest<OrderResponse>;
