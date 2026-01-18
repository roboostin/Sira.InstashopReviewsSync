
using System.Linq.Expressions;
using API.Domain.Entities;
using API.Infrastructure.Persistence.DbContexts;
using API.Shared.Models;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace API.Infrastructure.Persistence.Repositories;



public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly IdGenerator _idGenerator;
    readonly UserState _userState;

    public Repository(ApplicationDbContext context, IdGenerator idGenerator,UserState userState)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _idGenerator = idGenerator;
        _userState = userState;
    }

    private IQueryable<T> ApplyCompanyFilter(IQueryable<T> query)
    {
        // Only apply company filter if CompanyID has a valid value
        if (_userState?.CompanyID > 0)
        {
            return query.Where(e => e.CompanyID == _userState.CompanyID);
        }
        return query;
    }

    public IQueryable<T> Get(Expression<Func<T?, bool>> predicate = null)
    {
        var query = _dbSet.Where(entity => !entity.IsDeleted);
        query = ApplyCompanyFilter(query);
        return predicate == null ? query : query.Where(predicate);
    }
    public IQueryable<T?> GetNotDeleted()
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        return ApplyCompanyFilter(query);
    }
    public async Task<T?> FirstOrDefaultAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetNotDeleted().FirstOrDefaultAsync(e => e.ID == id, cancellationToken).ConfigureAwait(false);
    }
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T?, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null ? await GetNotDeleted().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false) : await GetNotDeleted().Where(predicate).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }
    // public async Task<IEnumerable<T?>> GetAllAsync(Expression<Func<T?, bool>> predicate = null, CancellationToken cancellationToken = default)
    // {
    //     return predicate == null ? await GetNotDeleted().ToListAsync(cancellationToken).ConfigureAwait(false) : await GetNotDeleted().Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    // }


    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetNotDeleted().AnyAsync(e => e.ID == id, cancellationToken).ConfigureAwait(false);
    }


    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null ? await GetNotDeleted().AnyAsync(cancellationToken) : await GetNotDeleted().AnyAsync(predicate, cancellationToken);
    }
    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null, CancellationToken cancellationToken = default)
    {
        return predicate == null ? await GetNotDeleted().CountAsync(cancellationToken) : await GetNotDeleted().CountAsync(predicate, cancellationToken);
    }
    public long Add(T entity)
    {
        
        _dbSet.Add(entity);
        return entity.ID;
    }
    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Attach(entity);

        var entry = _context.Entry(entity);
        entry.Property(e => e.CreatedAt).IsModified = false;
        entry.Property(e => e.CreatedBy).IsModified = false;
        entry.Property(e => e.UpdatedBy).IsModified = true;
        entry.Property(e => e.UpdatedAt).IsModified = true;
        entry.State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.RowVersion = new byte[8];

        var entry = _context.Entry(entity);
        entry.Property(e => e.CreatedAt).IsModified = false;
        entry.Property(e => e.CreatedBy).IsModified = false;
        entry.Property(e => e.IsDeleted).IsModified = true;
        entry.Property(e => e.UpdatedBy).IsModified = true;
        entry.Property(e => e.UpdatedAt).IsModified = true;
        entry.Property(e => e.RowVersion).IsModified = true;
    }
    public virtual void SaveIncluded(T entity, params string[] properties)
    {
        var local = _context.Set<T>()
            .Local.FirstOrDefault(entry => entry.ID == entity.ID);

        EntityEntry entry;

        if (local is null)
            entry = _context.Entry(entity);
        else
            entry = _context.ChangeTracker.Entries<T>().FirstOrDefault(e => e.Entity.ID == entity.ID);

        foreach (var prop in entry.Properties)
        {
            if (properties.Contains(prop.Metadata.Name))
            {
                prop.CurrentValue = entity.GetType().GetProperty(prop.Metadata.Name).GetValue(entity);
                prop.IsModified = true;
            }
        }

        // entity.UpdatedBy = _unitOfWork.UserID;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.RowVersion = new byte[8];
        entry.Property("RowVersion").IsModified = true;

    }
    public void UpdatePartial(T entity, params Expression<Func<T, object>>[] updatedProperties)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Attach(entity);

        _context.Entry(entity).Property(e => e.UpdatedBy).IsModified = true;
        _context.Entry(entity).Property(e => e.UpdatedAt).IsModified = true;

        foreach (var property in updatedProperties)
        {
            _context.Entry(entity).Property(property).IsModified = true;
        }
    }
    //public void UpdateExcluding(T entity, params Expression<Func<T, object>>[] excludedProperties)
    //{

    //    entity.UpdatedAt = DateTime.UtcNow;
    //    _dbSet.Attach(entity);

    //    _context.Entry(entity).Property(e => e.UpdatedBy).IsModified = true;
    //    _context.Entry(entity).Property(e => e.UpdatedAt).IsModified = true;

    //    var entry = _context.Entry(entity);
    //    var properties = entry.Properties;

    //    // Get the property names to exclude
    //    var excludedPropertyNames = excludedProperties
    //        .Select(exp => GetPropertyName(exp))
    //        .ToList();

    //    // Mark all properties as modified except the excluded ones
    //    foreach (var property in properties)
    //    {
    //        if (!excludedPropertyNames.Contains(property.Metadata.Name))
    //        {
    //            property.IsModified = true;
    //        }
    //    }
    //    _context.Entry(entity).Property(e => e.ID).IsModified = false;

    //                _context.Entry(entity).Property(e => e.CreatedAt).IsModified = false;
    //                _context.Entry(entity).Property(e => e.CreatedBy).IsModified = false;
    //}
    public void UpdateExcluding(T entity, params Expression<Func<T, object>>[] excludedProperties)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.UpdatedAt = DateTime.UtcNow;

        if (_context.Entry(entity).State == EntityState.Detached)
            _dbSet.Attach(entity);

        var entry = _context.Entry(entity);

        // Always exclude these properties from modification
        var alwaysExcluded = new List<string>
        {
            nameof(BaseEntity.ID),
            nameof(BaseEntity.CreatedAt),
            nameof(BaseEntity.CreatedBy)
        };

        // Add navigation properties that are part of the key
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(p => p.Name)
            .ToList() ?? new List<string>();

        alwaysExcluded.AddRange(keyProperties);

        // Get names of properties to exclude from parameters
        var excludedPropertyNames = excludedProperties
            .Select(GetPropertyName)
            .Concat(alwaysExcluded)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Force update of audit fields
        entry.Property(e => e.UpdatedAt).IsModified = true;
        entry.Property(e => e.UpdatedBy).IsModified = true;

        // Mark all non-excluded properties as modified
        foreach (var property in entry.Properties)
        {
            property.IsModified = !excludedPropertyNames.Contains(property.Metadata.Name);
        }
    }

    private string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (memberExpression == null)
        {
            var unaryExpression = propertyExpression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
        }
        return memberExpression?.Member.Name;
    }


    public async Task<int> BatchDelete(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await GetNotDeleted().Where(predicate).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
    }
    public void AddRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.ID = _idGenerator.CreateId(); // Generate Snowflake ID
            entity.CreatedBy = entity.CreatedBy == 0?  _userState.UserID : entity.CreatedBy;
            entity.CompanyID = entity.CompanyID == 0 ? _userState.CompanyID : entity.CompanyID;
        }
        _dbSet.AddRange(entities);
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await Get().AnyAsync(predicate).ConfigureAwait(false);
    }

    public async Task<int> BulkUpdateAsync(Expression<Func<T, bool>> predicate,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression,
        CancellationToken cancellationToken = default)
    {
        return await GetNotDeleted()
            .Where(predicate)
            .ExecuteUpdateAsync(updateExpression, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<int> BulkUpdateAsync(Expression<Func<T, bool>> predicate, Action<T> updateAction, CancellationToken cancellationToken = default)
    {
        var entities = await GetNotDeleted()
            .Where(predicate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (var entity in entities)
        {
            updateAction(entity);
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return entities.Count;
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<T> entities, Action<T> updateAction, CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        var currentTime = DateTime.UtcNow;

        foreach (var entity in entityList)
        {
            updateAction(entity);
            entity.UpdatedAt = currentTime;
        }

        _dbSet.UpdateRange(entityList);
        return entityList.Count;
    }

    public void SaveChanges()
    {
        _context.SaveChanges();
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);   
    }
}
