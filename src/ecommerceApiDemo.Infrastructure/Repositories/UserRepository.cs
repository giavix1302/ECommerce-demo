using ecommerceApiDemo.Application.Common.Interfaces;
using ecommerceApiDemo.Infrastructure.Persistence;
using ecommerceApiDemo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ecommerceApiDemo.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _dbSet.AnyAsync(u => u.Email == email);
}
