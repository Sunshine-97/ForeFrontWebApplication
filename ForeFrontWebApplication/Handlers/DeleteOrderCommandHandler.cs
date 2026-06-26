using ForeFrontWebApplication.Commands;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class DeleteOrderCommandHandler(IOrderRepository orders)
    : IRequestHandler<DeleteOrderCommand, bool>
{
    public Task<bool> Handle(DeleteOrderCommand cmd, CancellationToken ct)
        => orders.DeleteAsync(cmd.OrderId, ct);
}
