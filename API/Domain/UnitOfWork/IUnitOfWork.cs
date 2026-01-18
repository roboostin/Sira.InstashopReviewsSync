using API.Infrastructure.Persistence.DbContexts;

namespace API.Domain.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    ApplicationDbContext _context { get; set; }
    Task<bool> SaveChangesAsync();
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task CommitChangesAsync();
    Task RollbackTransactionAsync();
}
