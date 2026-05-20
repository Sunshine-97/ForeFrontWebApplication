using ForeFrontWebApplication.Data;
using CustomerModel = ForeFrontWebApplication.Models.Customer.Customers;
using Microsoft.EntityFrameworkCore;

namespace ForeFrontWebApplication.Repositories.Customer;

public sealed class EfCustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public EfCustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CustomerModel?> GetByIdAsync(string customerId, CancellationToken ct = default) =>
        await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

    public async Task<CustomerModel?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task<bool> ExistsAsync(string customerId, CancellationToken ct = default) =>
        await _db.Customers.AnyAsync(c => c.CustomerId == customerId, ct);

    public async Task AddAsync(CustomerModel customer, CancellationToken ct = default)
    {
        await _db.Customers.AddAsync(customer, ct);
        await _db.SaveChangesAsync(ct);
    }
}
