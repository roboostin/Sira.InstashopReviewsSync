
using System.Linq.Expressions;
using API.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Infrastructure.Persistence.Repositories;

public interface IRepository<T> where T : BaseEntity, new()
{
    void UpdateExcluding(T entity, params Expression<Func<T, object>>[] excludedProperties);

    IQueryable<T> Get(Expression<Func<T?, bool>> predicate = null);
    Task<T?> FirstOrDefaultAsync(long id, CancellationToken cancellationToken = default);

      Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate = null,
        CancellationToken cancellationToken = default);
    //Task<IEnumerable<T?>> GetAllAsync(Expression<Func<T?, bool>> predicate = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T?, bool>> predicate = null, CancellationToken cancellationToken = default);
   Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default);
   long Add(T entity);
    void Update(T entity);
    void SaveIncluded(T entity, params string[] properties);
    void UpdatePartial(T entity, params Expression<Func<T, object>>[] updatedProperties);
    void Delete(T entity);
    Task<int> BatchDelete(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    void AddRange(IEnumerable<T> entities);
    
    // Bulk Update Methods
    Task<int> BulkUpdateAsync(Expression<Func<T, bool>> predicate,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression,
        CancellationToken cancellationToken = default);
    Task<int> BulkUpdateAsync(Expression<Func<T, bool>> predicate, Action<T> updateAction, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateAsync(IEnumerable<T> entities, Action<T> updateAction, CancellationToken cancellationToken = default);

    void SaveChanges();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}