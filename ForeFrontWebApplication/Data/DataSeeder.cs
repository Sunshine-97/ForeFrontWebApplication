using System.Reflection;
using System.Text.Json;
using ForeFrontWebApplication.Models.Customer;
using ForeFrontWebApplication.Models.Order;
using ForeFrontWebApplication.Models.Product;
using ForeFrontWebApplication.Repositories.Customer;
using ForeFrontWebApplication.Repositories.Order;
using ForeFrontWebApplication.Repositories.Product;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Data;

public static class DataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var products    = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var customers   = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var orders      = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var logger      = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        var seedData = LoadSeedData();
        if (seedData is null)
        {
            logger.LogWarning("DataSeeder: seeddata.json could not be loaded. Skipping seed.");
            return;
        }

        await SeedProductsAsync(context, products, seedData, logger);
        await SeedCustomersAsync(context, customers, seedData, logger);
        await SeedOrdersAsync(context, customers, orders, seedData, logger);
    }

    private static async Task SeedProductsAsync(
        AppDbContext context,
        IProductRepository productRepo,
        SeedRoot seedData,
        ILogger logger)
    {
        if (await context.Products.AnyAsync())
            return;

        var products = seedData.Orders
            .SelectMany(o => o.Produkter)
            .GroupBy(p => p.ProductId)
            .Select(g => new Products
            {
                ProductId = g.Key,
                Namn      = g.First().Namn,
                Pris      = g.First().Pris,
            })
            .ToList();

        foreach (var product in products)
            await productRepo.AddAsync(product);

        logger.LogInformation("DataSeeder: seeded {Count} products.", products.Count);
    }

    private static async Task SeedCustomersAsync(
        AppDbContext context,
        ICustomerRepository customerRepo,
        SeedRoot seedData,
        ILogger logger)
    {
        if (await context.Customers.AnyAsync())
            return;

        var customers = seedData.Orders
            .GroupBy(o => o.Kund.Email)
            .Select(g => new Customers
            {
                CustomerId = Guid.NewGuid().ToString(),
                Namn       = g.First().Kund.Namn,
                Email      = g.Key,
            })
            .ToList();

        foreach (var customer in customers)
            await customerRepo.AddAsync(customer);

        logger.LogInformation("DataSeeder: seeded {Count} customers.", customers.Count);
    }

    private static async Task SeedOrdersAsync(
        AppDbContext context,
        ICustomerRepository customerRepo,
        IOrderRepository orderRepo,
        SeedRoot seedData,
        ILogger logger)
    {
        if (await context.Orders.AnyAsync())
            return;

        var customerLookup = await context.Customers
            .ToDictionaryAsync(c => c.Email, c => c.CustomerId);

        var orders = seedData.Orders.Select(o => new OrderEntity
        {
            OrderId   = o.OrderId,
            KundId    = customerLookup[o.Kund.Email],
            Status    = ParseStatus(o.Status),
            Created   = DateTime.SpecifyKind(o.Datum, DateTimeKind.Utc),
            Produkter = o.Produkter.Select(p => new OrderLine
            {
                OrderLineId = Guid.NewGuid().ToString(),
                OrderId     = o.OrderId,
                ProductId   = p.ProductId,
                Namn        = p.Namn,
                Pris        = p.Pris,
                Antal       = p.Antal,
            }).ToList(),
        }).ToList();

        foreach (var order in orders)
            await orderRepo.AddAsync(order);

        logger.LogInformation("DataSeeder: seeded {Count} orders.", orders.Count);
    }

    private static OrderStatus ParseStatus(string status) =>
        status.ToLowerInvariant() switch
        {
            "pending"   => OrderStatus.Pending,
            "confirmed" => OrderStatus.Confirmed,
            "shipped"   => OrderStatus.Shipped,
            "delivered" => OrderStatus.Delivered,
            "cancelled" => OrderStatus.Cancelled,
            _           => OrderStatus.Pending,
        };

    private static SeedRoot? LoadSeedData()
    {
        var assembly     = Assembly.GetExecutingAssembly();
        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("seeddata.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            return null;

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return JsonSerializer.Deserialize<SeedRoot>(json, JsonOptions);
    }

    private sealed class SeedRoot
    {
        public List<SeedOrder> Orders { get; set; } = [];
    }

    private sealed class SeedOrder
    {
        public string         OrderId  { get; set; } = string.Empty;
        public SeedKund       Kund     { get; set; } = new();
        public string         Status   { get; set; } = string.Empty;
        public DateTime       Datum    { get; set; }
        public List<SeedLine> Produkter { get; set; } = [];
    }

    private sealed class SeedKund
    {
        public string Namn  { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private sealed class SeedLine
    {
        public string  ProductId { get; set; } = string.Empty;
        public string  Namn      { get; set; } = string.Empty;
        public int     Antal     { get; set; }
        public decimal Pris      { get; set; }
    }
}
