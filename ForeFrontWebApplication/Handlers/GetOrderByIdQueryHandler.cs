using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Queries;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetOrderByIdQuery, Orders?>
{
    public Task<Orders?> Handle(GetOrderByIdQuery query, CancellationToken ct)
        => orders.GetByIdAsync(query.OrderId, ct);
}
