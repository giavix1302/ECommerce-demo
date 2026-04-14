using Application.Common.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ICartRepository Carts { get; }
    public IOrderRepository Orders { get; }
    public ICouponRepository Coupons { get; }
    public IReviewRepository Reviews { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Products = new ProductRepository(context);
        Categories = new CategoryRepository(context);
        Carts = new CartRepository(context);
        Orders = new OrderRepository(context);
        Coupons = new CouponRepository(context);
        Reviews = new ReviewRepository(context);
    }

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
            await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
