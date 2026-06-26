using FluentValidation;
using ForeFrontWebApplication.Commands;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Repositories;

namespace ForeFrontWebApplication.Shared.Behaviors;

<<<<<<< HEAD
public class UpdateOrderValidator : AbstractValidator<UpdateOrderStatusCommand>
=======
internal sealed class UpdateOrderValidator : AbstractValidator<UpdateOrderStatusCommand>
>>>>>>> 7726cb5f43fa24672b425b11376930eed481072c
{
    // Defines the permitted forward-only state machine transitions.
    // Delivered and Cancelled are terminal states — no further transitions allowed.
    private static readonly Dictionary<OrderStatus, OrderStatus[]> _allowedTransitions = new()
    {
        [OrderStatus.Pending]   = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipped,   OrderStatus.Cancelled],
        [OrderStatus.Shipped]   = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = [],
    };

    public UpdateOrderValidator(IOrderRepository orders)
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid order status value.");

        RuleFor(x => x.OrderId)
            .MustAsync(async (orderId, ct) =>
                await orders.GetByIdAsync(orderId, ct) is not null)
            .WithMessage("Order not found.");

        RuleFor(x => x.Status)
            .MustAsync(async (command, status, ct) =>
            {
                var order = await orders.GetByIdAsync(command.OrderId, ct);
                return order is null
                    || (_allowedTransitions.TryGetValue(order.Status, out var allowed)
                        && allowed.Contains(status));
            })
            .WithMessage(
                "Invalid status transition. " +
                "Allowed transitions: Pending→Confirmed/Cancelled, " +
                "Confirmed→Shipped/Cancelled, Shipped→Delivered.");
    }
}
