// API/Extensions/FilterExtensions.cs
using API.Application.Filters;
using Microsoft.AspNetCore.Builder;

namespace API.Extensions;

public static class FilterExtensions
{
    public static RouteHandlerBuilder AddCancellationTimeZoneFilter(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<CaptureCancellationAndTimeZoneFilter>();
    }
}