using ForeFrontWebApplication.Models;

namespace ForeFrontWebApplication.Repositories
{
  public interface ICustomerRepository
  {
    Task<Customer?> GetByIdAsync(string customerId, CancellationToken ct = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(string customerId, CancellationToken ct = default);
    Task AddAsync(Customer customer, CancellationToken ct = default);
  }
}
