using ForeFrontWebApplication.Models.Customer;

namespace ForeFrontWebApplication.Repositories.Customer;

public interface ICustomerRepository
{
    Task<Models.Customer.Customers?> GetByIdAsync(string customerId, CancellationToken ct = default);
    Task<Models.Customer.Customers?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(string customerId, CancellationToken ct = default);
    Task AddAsync(Models.Customer.Customers customer, CancellationToken ct = default);
}
