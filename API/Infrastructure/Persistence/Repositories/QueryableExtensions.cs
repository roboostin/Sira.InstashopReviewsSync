using API.Shared.Helpers;

namespace API.Infrastructure.Persistence.Repositories;

using System;
using System.Linq;
using System.Linq.Expressions;

public static class QueryableExtensions
{
    public static IQueryable<T> ConvertToUserTimeZone<T>(
        this IQueryable<T> query) where T : class
    {
        string? userTimeZone = HttpRequestHelper.GetTimeZone();

        if (string.IsNullOrEmpty(userTimeZone))
            return query; // No conversion if no time zone is found

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);

        var parameter = Expression.Parameter(typeof(T), "x");
        var bindings = typeof(T).GetProperties()
            .Select(p =>
            {
                if (p.PropertyType == typeof(DateTime))
                {
                    // Convert DateTime fields from UTC to user's time zone
                    var propertyAccess = Expression.Property(parameter, p);
                    var convertMethod = typeof(TimeZoneInfo).GetMethod(nameof(TimeZoneInfo.ConvertTimeFromUtc),
                        new[] { typeof(DateTime), typeof(TimeZoneInfo) });

                    var timeZoneExpression = Expression.Constant(timeZone, typeof(TimeZoneInfo));
                    var conversionExpression = Expression.Call(convertMethod!, propertyAccess, timeZoneExpression);
                    return Expression.Bind(p, conversionExpression);
                }
                return Expression.Bind(p, Expression.Property(parameter, p));
            }).ToList();

        var memberInit = Expression.MemberInit(Expression.New(typeof(T)), bindings);
        var lambda = Expression.Lambda<Func<T, T>>(memberInit, parameter);

        return query.Select(lambda);
    }

   
}
