# CQRS + MediatR Implementation Directives

## Core Rules

- Every use-case = one `IRequest<T>` (command or query) + one `IRequestHandler<TRequest, TResponse>`.
- **Commands** mutate state; **Queries** only read. Never mix.
- Controllers send requests via `IMediator.Send()`; they must not contain business logic.
- Services (`OrderService`, `WarehouseService`, `AuthService`) are dissolved into handlers — do **not** keep parallel service + handler for the same concern.

## Folder Structure

```
  Orders/
    Commands/
      Create/
        CreateOrderCommand.cs
        CreateOrderCommandHandler.cs
      Update/
        UpdateOrderStatusCommand.cs
        UpdateOrderStatusCommandHandler.cs
      Delete/
        DeleteOrderCommand.cs (Empty for now, can be implemented later if needed)
        DeleteOrderCommandHandler.cs (Empty for now, can be implemented later if needed)
    Queries/
      GetAllOrders/
        GetAllOrdersQuery.cs
        GetAllOrdersQueryHandler.cs
      GetOrderById/
        GetOrderByIdQuery.cs
        GetOrderByIdQueryHandler.cs
  Warehouse/
    Queries/
      ...
```

## Naming

| Type | Pattern | Example |
|------|---------|---------|
| Command | `<Verb><Entity>Command` | `CreateOrderCommand` |
| Query | `Get<Entity>[By<Key>]Query` | `GetOrderByIdQuery` |
| Handler | `<RequestName>Handler` | `CreateOrderCommandHandler` |

## Request/Handler Skeleton

```csharp
// Command
public sealed record CreateOrderCommand(string KundId, List<OrderItemRequest> Produkter)
    : IRequest<OrderResponse>;

// Handler
public sealed class CreateOrderCommandHandler(
    IOrderRepository orders,
    ICustomerRepository customers,
    IProductRepository products)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand cmd, CancellationToken ct)
    { ... }
}
```

## Controller Pattern

```csharp
// Inject IMediator only — no service interfaces
public OrdersController(IMediator mediator) { ... }

[HttpPost]
public async Task<IActionResult> Create([FromBody] OrderRequest req, CancellationToken ct)
    => CreatedAtAction(..., await _mediator.Send(new CreateOrderCommand(req.KundId, req.Produkter), ct));
```

## Registration (Program.cs)

```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<Program>());
```

Repositories remain registered as-is; only service registrations are removed as handlers replace them.

## Validation

- Use `FluentValidation` with a MediatR `IPipelineBehavior<TRequest, TResponse>` for automatic validation before handlers run.
- Keep validators co-located with their command/query.

## Error Handling

- Handlers throw domain exceptions (`KeyNotFoundException`, custom exceptions).
- Map exceptions to HTTP responses in a single global `IPipelineBehavior` or `ExceptionHandler` middleware — not in controllers.

## What NOT to Do

- Do not inject `IOrderService` (or any dissolved service) into controllers after migration.
- Do not put EF Core `DbContext` directly in handlers — always go through repositories.
- Do not create "catch-all" handlers covering multiple use-cases.
