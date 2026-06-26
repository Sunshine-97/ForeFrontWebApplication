using ForeFrontWebApplication.Commands;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class UpdateOrderStatusCommandHandler(IOrderRepository orders)
    : IRequestHandler<UpdateOrderStatusCommand, Orders?>
{
    public async Task<Orders?> Handle(UpdateOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct);
        if (order is null)
            return null;

        order.Status = cmd.Status;
        await orders.UpdateAsync(order, ct);
        return order;
    }
}
