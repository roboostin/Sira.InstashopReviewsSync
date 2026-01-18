using API.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace API.Domain.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public ApplicationDbContext _context { get; set; }
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Dispose()
    {
         if (_context?.Database?.CurrentTransaction is not null)
                _context.Database.CloseConnection();
            _context?.Dispose();
    }

    public async Task BeginTransactionAsync()
    {
        _context.Database.CloseConnection();

        if (_context.Database.CurrentTransaction != null || !_context.Database.CanConnect())
            return;
        
        await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadUncommitted);

    }
    public async Task<bool> SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task CommitTransactionAsync()
    {
        if (_context.Database.CurrentTransaction == null)
                return;

        await _context.Database.CommitTransactionAsync();
    }

    public async Task CommitChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            await CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await RollbackTransactionAsync();
            throw ex;
        }
}

    public async Task RollbackTransactionAsync()
    {
        if (_context.Database.CurrentTransaction == null)
            return;

        await _context.Database.RollbackTransactionAsync();
        _context.ChangeTracker.Clear();
    }
}
