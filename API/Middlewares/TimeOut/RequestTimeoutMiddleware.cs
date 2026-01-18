using System.Diagnostics;

public class RequestTimeoutMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimeoutMiddleware> _logger;

    public RequestTimeoutMiddleware(RequestDelegate next, ILogger<RequestTimeoutMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        int timeoutSeconds = 20; // Default timeout

        var timeoutAttr = endpoint?.Metadata.GetMetadata<TimeOutAttribute>();

        if (timeoutAttr != null)
        {
            timeoutSeconds = timeoutAttr.TimeoutSeconds;

            if (timeoutSeconds <= 0)
            {
                _logger.LogInformation("TimeOut: Request to {Path} has NO timeout.", context.Request.Path);
                await _next(context);
                return;
            }

            _logger.LogInformation("TimeOut: Request to {Path} has a custom timeout of {Timeout} seconds.",
                context.Request.Path, timeoutSeconds);
        }

        using var timeoutCts = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
            ? new CancellationTokenSource(TimeSpan.FromSeconds(999))
            : new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, context.RequestAborted);
    
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next.Invoke(context).WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            
            // Check if this was due to our timeout or client disconnect
            if (timeoutCts.IsCancellationRequested)
            {
                // Timeout occurred - handle it gracefully
                _logger.LogWarning("Request to {Path} timed out after {ElapsedMilliseconds}ms", 
                    context.Request.Path, stopwatch.ElapsedMilliseconds);

                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
                    await context.Response.WriteAsync("Request timed out.");
                }
            }
            else
            {
                // Other cancellations (e.g., client disconnect) - don't log as error
                // Just let it be - the response won't be sent anyway if client disconnected
                _logger.LogDebug("Request to {Path} was cancelled (likely client disconnect) after {ElapsedMilliseconds}ms", 
                    context.Request.Path, stopwatch.ElapsedMilliseconds);
                // Don't re-throw - this prevents it from being logged as unhandled exception
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class TimeOutAttribute : Attribute
{
    public int TimeoutSeconds { get; }
    public TimeOutAttribute(int timeoutSeconds)
    {
        TimeoutSeconds = timeoutSeconds;
    }
}

public static class RequestTimeoutMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTimeout(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTimeoutMiddleware>();
    }
    public static RouteHandlerBuilder WithTimeout(this RouteHandlerBuilder builder, int timeoutSeconds)
    {
        return builder.WithMetadata(new TimeOutAttribute(timeoutSeconds));
    }
}

