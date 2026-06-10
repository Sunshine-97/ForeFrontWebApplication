using ForeFrontWebApplication.Data;
using ForeFrontWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Repositories
{
  public sealed class CustomerRepository : ICustomerRepository
  {
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
      _db = db;
    }

    public async Task<Customer?> GetByIdAsync(string customerId, CancellationToken ct = default) =>
        await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task<bool> ExistsAsync(string customerId, CancellationToken ct = default) =>
        await _db.Customers.AnyAsync(c => c.CustomerId == customerId, ct);

    public async Task AddAsync(Customer customer, CancellationToken ct = default)
    {
      await _db.Customers.AddAsync(customer, ct);
      await _db.SaveChangesAsync(ct);
    }
  }
}
