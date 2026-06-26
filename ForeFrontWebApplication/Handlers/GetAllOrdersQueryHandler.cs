using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Queries;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class GetAllOrdersQueryHandler(IOrderRepository orders)
    : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<Orders>>
{
    public Task<IReadOnlyList<Orders>> Handle(GetAllOrdersQuery query, CancellationToken ct)
        => orders.GetAllAsync(ct);
}
