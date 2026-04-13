using ecommerceApiDemo.Domain.Entities;

namespace ecommerceApiDemo.Application.Common.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
}
