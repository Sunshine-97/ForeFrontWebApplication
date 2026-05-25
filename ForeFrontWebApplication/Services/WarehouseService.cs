using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Warehouse;
using ForeFrontWebApplication.Repositories.Warehouse;

namespace ForeFrontWebApplication.Services;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<OrderVolumes>> GetVolumesAsync(
        DateTime? from = null,
        DateTime? to   = null,
        CancellationToken ct = default)
    {
        var orders = await _repository.GetDeliveredOrdersAsync(from, to, ct);
        return AggregateProducts(orders);
    }

    public async Task<IReadOnlyList<OrderVolumes>> GetTopProductsAsync(CancellationToken ct = default)
    {
        var orders = await _repository.GetDeliveredOrdersAsync(ct: ct);
        return AggregateProducts(orders).Take(10).ToList().AsReadOnly();
    }

    private static IReadOnlyList<OrderVolumes> AggregateProducts(IEnumerable<OrderEntity> orders)
    {
        return orders
            .SelectMany(order => order.Produkter)
            .GroupBy(line => line.ProductId)
            .Select(group => new OrderVolumes
            {
                ProduktId = group.Key,
                Namn      = group.First().Namn,
                Antal     = group.Sum(l => l.Antal),
            })
            .OrderByDescending(volume => volume.Antal)
            .ToList()
            .AsReadOnly();
    }
}
