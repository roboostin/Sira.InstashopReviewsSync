using System.Text.Json;

namespace API.Middlewares;
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. "+ex.ToString());
           // var provider = context.Items["ActionArgumentsProvider"] as IActionArgumentsProvider;

            var msg = $"{ex.Message} - {ex.InnerException?.Message}\n=============  \n=============";
            Console.WriteLine(msg);
            _logger.LogError(ex, msg);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Define a default error response
        var errorResponse = new
        {
            Message = "An unexpected error occurred.",
            Details = exception.Message,  // ⚠️ Avoid exposing sensitive information in production
            StatusCode = 200
        };

        context.Response.StatusCode = errorResponse.StatusCode;
        var jsonResponse = JsonSerializer.Serialize(errorResponse);

        return context.Response.WriteAsync(jsonResponse);
    }
}