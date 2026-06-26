using MediatR;

namespace ForeFrontWebApplication.Commands;

public sealed record DeleteOrderCommand(string OrderId) : IRequest<bool>;
