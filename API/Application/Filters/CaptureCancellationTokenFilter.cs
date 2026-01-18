// using API.Application.Interfaces;
// using Microsoft.AspNetCore.Mvc.Filters;
//
// namespace API.Application.Filters;
//
// public class CaptureCancellationTokenFilter : IActionFilter
// {
//     private readonly ICancellationTokenProvider _cancellationTokenProvider;
//     public CaptureCancellationTokenFilter(
//         ICancellationTokenProvider cancellationTokenProvider)
//     {
//         _cancellationTokenProvider = cancellationTokenProvider;
//     }
//
//     public void OnActionExecuting(ActionExecutingContext context)
//     {
//         string timeZoneId = context.HttpContext?.Request.Headers["TimeZone"].FirstOrDefault() ?? "Africa/Cairo";
//
//         _cancellationTokenProvider.CaptureTimeZone(timeZoneId);
//         _cancellationTokenProvider.CaptureToken(context.HttpContext.RequestAborted);
//
//     }
//
//     public void OnActionExecuted(ActionExecutedContext context)
//     {
//     }
// }

// API/Application/Filters/CaptureCancellationAndTimeZoneFilter.cs
using API.Application.Interfaces;

namespace API.Application.Filters;

// The new filter implementing IEndpointFilter
public class CaptureCancellationAndTimeZoneFilter : IEndpointFilter
{
    private readonly ICancellationTokenProvider _cancellationTokenProvider;

    // The dependency is injected via the constructor, just like before
    public CaptureCancellationAndTimeZoneFilter(ICancellationTokenProvider cancellationTokenProvider)
    {
        _cancellationTokenProvider = cancellationTokenProvider;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // --- This is the equivalent of "OnActionExecuting" ---

        // 1. Get the HttpContext from the EndpointFilterInvocationContext
        var httpContext = context.HttpContext;

        // 2. Read the "TimeZone" header
        string timeZoneId = httpContext.Request.Headers["TimeZone"].FirstOrDefault() ?? "Africa/Cairo";

        // 3. Capture the TimeZone and the CancellationToken
        _cancellationTokenProvider.CaptureTimeZone(timeZoneId);
        _cancellationTokenProvider.CaptureToken(httpContext.RequestAborted);

        // --- Call the next filter or the endpoint handler itself ---
        return await next(context);

        // --- Code here would be the equivalent of "OnActionExecuted" ---
    }
}