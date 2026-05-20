# Repository Directives

Data access layer rules for the Repository pattern with EF Core and PostgreSQL (Docker).

---

## Architecture

- Define an interface and concrete class per repository (e.g. `IOrderRepository` / `OrderRepository`) under `Repositories/<Domain>/`.
- Register as `AddScoped<IInterface, TImpl>()` in `Program.cs`.
- Never inject `DbContext` directly into services or controllers.
- No business logic in repositories — data access only.

### Folder Structure

```
Data/
  AppDbContext.cs
  Configurations/
Repositories/
  Orders/
  Warehouse/
Migrations/
```

---

## Docker + PostgreSQL

- Define the PostgreSQL service in `docker-compose.yml` at the repo root.
- Use environment variables for all credentials (`.env` file, git-ignored). Never hardcode.
- Use a named volume to persist data.

```yaml
services:
  db:
    image: postgres:16
    restart: unless-stopped
    environment:
      POSTGRES_USER:     ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB:       ${POSTGRES_DB}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
volumes:
  postgres_data:
```

- Connection string in `appsettings.Development.json` (local only); production via env var/secrets manager.
- Key: `ConnectionStrings:DefaultConnection`.

---

## EF Core Setup

**Packages:**
```
Microsoft.EntityFrameworkCore 8.x
Npgsql.EntityFrameworkCore.PostgreSQL 8.x
Microsoft.EntityFrameworkCore.Design 8.x
```

**`AppDbContext`** in `Data/AppDbContext.cs`:
```csharp
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Order> Orders { get; init; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

**`Program.cs`:**
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.")));
```

---

## Entity Configuration

- One `IEntityTypeConfiguration<T>` class per entity — never inline in `OnModelCreating`.
- Use snake_case table and column names (PostgreSQL convention).
- Explicitly set primary keys, required constraints, and all relationships.
- Store enum status columns as `string` via `.HasConversion<string>()`.

---

## Repository Rules

- All DB calls must be `async`/`await` with `CancellationToken`.
- Return materialised domain types — never expose `IQueryable<T>`.
- Never call `SaveChangesAsync` inside a repository — call it from the service layer.
- Use `AsNoTracking()` for read-only queries.

---

## Model Mapping

```
Controller → Service → Repository → Database
   DTO        Domain Model           Entity
```
- Map DTOs ↔ domain models in the service layer only.
- Never pass DTOs into repositories or entities to controllers.

---

## Migrations

- Use EF Core Migrations exclusively — no hand-written DDL.
- Name migrations descriptively: `dotnet ef migrations add AddOrdersTable`.
- Apply via `MigrateAsync()` at startup (non-prod) or CI/CD step (prod).
- Never use `EnsureCreated()`. Commit migration files to source control.

---

## Transactions

- Call `SaveChangesAsync(ct)` from the service layer.
- Wrap multi-repository operations in an explicit transaction:

```csharp
await using var tx = await _db.Database.BeginTransactionAsync(ct);
try { /* repo calls + SaveChangesAsync */ await tx.CommitAsync(ct); }
catch { await tx.RollbackAsync(ct); throw; }
```

---

## Security

- Never log connection strings.
- Use parameterised queries only (EF Core default). No `FromSqlRaw` with user input — use `FromSqlInterpolated`.
- Validate inputs in the service layer before calling repositories.
- DB user needs only SELECT/INSERT/UPDATE/DELETE — no DDL in production.

---

## Testing

- Never test against production or shared dev databases.
- Use `Testcontainers.PostgreSql` for integration tests.
- Reset/recreate DB state between mutating test runs.
- Do not rely solely on the EF in-memory provider — it doesn't enforce relational constraints.
