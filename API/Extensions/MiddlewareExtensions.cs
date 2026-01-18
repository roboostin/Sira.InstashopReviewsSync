using API.Middlewares;
using Serilog;

namespace API.Extensions;

public static class MiddlewareExtensions
{
    public static void ConfigureMiddlewares(this WebApplication app, IConfiguration configuration)
    {
        app.UseCors();
        app.UseMiddleware<SlowRequestLoggingMiddleware>(); // Register slow request middleware

        app.UseMiddleware<RequestTimeoutMiddleware>(); // Add the timeout middleware

        // Use Global Exception Handling Middleware
        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseSerilogRequestLogging();
        // Use Hangfire Dashboard
        app.UseHangfireDashboard(configuration);
        app.UseAuthentication(); // Enable JWT authentication
        app.UseAuthorization();
        app.ConfigureEndpoints();
    }
}