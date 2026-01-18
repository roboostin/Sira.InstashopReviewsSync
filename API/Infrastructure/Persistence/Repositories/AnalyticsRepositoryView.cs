using System.Linq.Expressions;
using API.Domain.Entities;
using API.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
namespace API.Infrastructure.Persistence.Repositories
{
    public class AnalyticsRepositoryView<T> : IAnalyticsRepositoryView<T> where T : class, new()
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public AnalyticsRepositoryView(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public bool Any(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ExecuteStoredProcedure(string procedure)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ExecuteStoredProcedure(string procedure, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> ExecuteTableFunction(string functionName, params object[] parameters)
        {
            string command = string.Join(",", parameters.Select(p => FormatParameter(p)).ToList());
            return _dbSet.FromSqlRaw($"select * from {functionName}({command})");
        }

        public T First(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T FirstOrDefault()
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Get()
        {
            return _dbSet;
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return predicate == null ? _dbSet : _dbSet.Where(predicate);
        }

        private static string FormatParameter(object parameter)
        {
            if (parameter == null)
                return "null";
            if (parameter is string && (string)parameter == "default")
                return $"{parameter}";
            if (parameter is string || parameter is TimeSpan)
                return $"'{parameter}'";
            if (parameter is DateTime)
            {
                var str = ((DateTime)parameter).ToString("yyyy-MM-dd hh:mm:ss");
                return $"'{str}'";
            }

            if (parameter is IEnumerable<long?> longs)
                return $"ARRAY[{string.Join(",", longs.Select(v => v?.ToString() ?? "null"))}]::bigint[]";

            if (parameter is IEnumerable<string> strings)
                return $"ARRAY[{string.Join(",", strings.Select(v => v == null ? "null" : $"'{v.Replace("'", "''")}'"))}]::text[]";

            if (parameter is IEnumerable<int?> ints)
                return $"ARRAY[{string.Join(",", ints.Select(v => v?.ToString() ?? "null"))}]::int[]";
            return parameter.ToString();
        }
    }
}
