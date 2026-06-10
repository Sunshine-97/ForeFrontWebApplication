using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Queries;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class GetTopProductsQueryHandler(IWarehouseRepository repository)
    : IRequestHandler<GetTopProductsQuery, IReadOnlyList<OrderVolumes>>
{
    public async Task<IReadOnlyList<OrderVolumes>> Handle(GetTopProductsQuery query, CancellationToken ct)
    {
        var orders = await repository.GetDeliveredOrdersAsync(ct: ct);
        return orders
            .SelectMany(o => o.Produkter)
            .GroupBy(line => line.ProductId)
            .Select(g => new OrderVolumes
            {
                ProduktId = g.Key,
                Namn      = g.First().Namn,
                Antal     = g.Sum(l => l.Antal),
            })
            .OrderByDescending(v => v.Antal)
            .Take(10)
            .ToList()
            .AsReadOnly();
    }
}
