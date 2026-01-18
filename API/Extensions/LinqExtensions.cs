using System.Linq.Expressions;
using System.Reflection;
using API.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class LinqExtensions
{
    public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string ordering, bool ascending = true)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type, "p");
        PropertyInfo property;
        Expression propertyAccess;
        if (ordering.Contains('.'))
        {
            String[] childProperties = ordering.Split('.');
            property = type.GetProperty(childProperties[0]);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
            for (int i = 1; i < childProperties.Length; i++)
            {
                property = property.PropertyType.GetProperty(childProperties[i]);
                propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
            }
        }
        else
        {
            property = typeof(T).GetProperty(ordering);
            propertyAccess = Expression.MakeMemberAccess(parameter, property);
        }
        var orderByExp = Expression.Lambda(propertyAccess, parameter);
        MethodCallExpression resultExp = Expression.Call(typeof(Queryable),
            ascending ? "OrderBy" : "OrderByDescending",
            new[] { type, property.PropertyType }, source.Expression,
            Expression.Quote(orderByExp));
        return source.Provider.CreateQuery<T>(resultExp);
    }
    
    public static async Task<PagingDto<T>> ToPagingDtoAsync<T>(this IQueryable<T> query, int pageIndex = 1, int pageSize = 100, CancellationToken cancellationToken = default)
    {
        var records = query.Count();
        if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
        var pages = (int)Math.Ceiling((double)records / pageSize);
        var excludedRows = (pageIndex - 1) * pageSize;
        var items = await query.Skip(excludedRows).Take(pageSize).ToListAsync(cancellationToken);
        return new (pageSize, pageIndex, records, pages, items);
    }


    public static PagingDto<T> ToPagingDto<T>(this IEnumerable<T> list, int pageIndex = 1, int pageSize = 100)
    {
        var records = list.Count();
        if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
        var pages = (int)Math.Ceiling((double)records / pageSize);
        var excludedRows = (pageIndex - 1) * pageSize;
        var items = list.Skip(excludedRows).Take(pageSize);
        return new(pageSize, pageIndex, records, pages, items);
    }
}