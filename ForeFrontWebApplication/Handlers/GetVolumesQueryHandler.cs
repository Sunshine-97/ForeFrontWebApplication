using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Queries;
using ForeFrontWebApplication.Repositories;
using MediatR;

namespace ForeFrontWebApplication.Handlers;

public sealed class GetVolumesQueryHandler(IWarehouseRepository repository)
    : IRequestHandler<GetVolumesQuery, IReadOnlyList<OrderVolumes>>
{
    public async Task<IReadOnlyList<OrderVolumes>> Handle(GetVolumesQuery query, CancellationToken ct)
    {
        var orders = await repository.GetDeliveredOrdersAsync(query.From, query.To, ct);
        return AggregateProducts(orders);
    }

    private static IReadOnlyList<OrderVolumes> AggregateProducts(IEnumerable<Orders> orders)
        => orders
            .SelectMany(o => o.Produkter)
            .GroupBy(line => line.ProductId)
            .Select(g => new OrderVolumes
            {
                ProduktId = g.Key,
                Namn      = g.First().Namn,
                Antal     = g.Sum(l => l.Antal),
            })
            .OrderByDescending(v => v.Antal)
            .ToList()
            .AsReadOnly();
}
