using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.Persistence.DbContexts;
using DotNetCore.CAP;
using DotNetCore.CAP.Filter;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Infrastructure.MessageBroker.Interceptors;

public class TransactionEventBusInterceptor(ApplicationDbContext dbContext, ICapPublisher capPublisher)
    : IEventBusInterceptor
{
    public double ExecutionOrder => 2; 

    private IDbContextTransaction? _transaction;
    
    public Task OnSubscribeExecutingAsync(ExecutingContext context1)
    {
        _transaction = dbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted, capPublisher, autoCommit: false);
        
        return Task.CompletedTask;
    }

    public async Task OnSubscribeExecutedAsync(ExecutedContext context1)
    {
        if (_transaction is not null)
        {
            try
            {
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                await _transaction.CommitAsync().ConfigureAwait(false);
            }
            catch(Exception exc)
            {

            }
            finally
            {
                await _transaction.DisposeAsync().ConfigureAwait(false);
                _transaction = null;
            }
        }
    }

    public Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        if (_transaction is not null)
        {
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        return Task.CompletedTask;
    }
}