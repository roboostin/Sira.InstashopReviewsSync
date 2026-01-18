namespace API.Infrastructure.Persistence.DbContexts;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DateTimeInterceptor : IMaterializationInterceptor, ISaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DateTimeInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is IHasDateTime entity)
        {
            string userTimeZone = GetUserTimeZoneFromHeader();
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);

            foreach (var property in instance.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(DateTime))
                {
                    var value = (DateTime)property.GetValue(instance);
                    if (value.Kind == DateTimeKind.Utc)
                    {
                        property.SetValue(instance, TimeZoneInfo.ConvertTimeFromUtc(value, timeZone));
                    }
                }
                else if (property.PropertyType == typeof(DateTime?))
                {
                    var value = (DateTime?)property.GetValue(instance);
                    if (value.HasValue && value.Value.Kind == DateTimeKind.Utc)
                    {
                        property.SetValue(instance, TimeZoneInfo.ConvertTimeFromUtc(value.Value, timeZone));
                    }
                }
            }
        }
        return instance;
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var entries = eventData.Context.ChangeTracker.Entries();
        foreach (var entry in entries)
        {
            if (entry.Entity is IHasDateTime entity)
            {
                foreach (var property in entry.Entity.GetType().GetProperties())
                {
                    if (property.PropertyType == typeof(DateTime))
                    {
                        var value = (DateTime)property.GetValue(entry.Entity);
                        if (value.Kind == DateTimeKind.Unspecified)
                        {
                            property.SetValue(entry.Entity, DateTime.SpecifyKind(value, DateTimeKind.Utc));
                        }
                        else if (value.Kind == DateTimeKind.Local)
                        {
                            property.SetValue(entry.Entity, value.ToUniversalTime());
                        }
                    }
                    else if (property.PropertyType == typeof(DateTime?))
                    {
                        var value = (DateTime?)property.GetValue(entry.Entity);
                        if (value.HasValue)
                        {
                            if (value.Value.Kind == DateTimeKind.Unspecified)
                            {
                                property.SetValue(entry.Entity, DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));
                            }
                            else if (value.Value.Kind == DateTimeKind.Local)
                            {
                                property.SetValue(entry.Entity, value.Value.ToUniversalTime());
                            }
                        }
                    }
                }
            }
        }
        return result;
    }

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        return SavingChanges(eventData, result);
    }

    private string GetUserTimeZoneFromHeader()
    {
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("TZ", out var timeZone) == true)
        {
            return timeZone.ToString();
        }
        return "UTC";
    }
}
