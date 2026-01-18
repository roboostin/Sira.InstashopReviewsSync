using System.Linq.Expressions;
using API.Domain.Entities;

namespace API.Infrastructure.Persistence.Repositories
{
    public interface IAnalyticsRepositoryView<Entity> where Entity : class, new()
    {
        IQueryable<Entity> Get();
        Entity FirstOrDefault(Expression<Func<Entity, bool>> predicate);
        Entity FirstOrDefault();
        bool Any(Expression<Func<Entity, bool>> predicate);
        IQueryable<Entity> Get(Expression<Func<Entity, bool>> predicate);

        Entity First(Expression<Func<Entity, bool>> predicate);

        IQueryable<Entity> ExecuteStoredProcedure(string procedure);
        IQueryable<Entity> ExecuteStoredProcedure(string procedure, params object[] parameters);
        IQueryable<Entity> ExecuteTableFunction(string functionName, params object[] parameters);
    }
}
