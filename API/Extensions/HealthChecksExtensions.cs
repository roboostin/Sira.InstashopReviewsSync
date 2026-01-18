using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
namespace API.Extensions;

public static class HealthChecksExtensions
{
    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ Add Health Checks
        services.AddHealthChecks()
            .AddCheck("API Health", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "api" })
            .AddNpgSql(
                connectionString: configuration.GetConnectionString("DefaultConnection"),
                name: "NpgSql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "database" }
            );
        
        // ✅ Load Health Check UI configuration from appsettings.json
        services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(10); // Refresh time for UI
            setup.MaximumHistoryEntriesPerEndpoint(50);
        }).AddInMemoryStorage(); // Store results in memory
        
    }

    public static void UseHealthChecks(this  WebApplication app)
    {

        // Map Public Health Check Endpoint
        // app.MapHealthChecks("/health");
// ✅ Public API Health Check
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("api"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        
// ✅ Database Health Check
        app.MapHealthChecks("/health/db", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        
        // ✅ Secure Database Health Check with API Key Middleware
        // app.Use(async (context, next) =>
        // {
        //     if (context.Request.Path.StartsWithSegments("/health/db"))
        //     {
        //         var apiKey = context.Request.Headers["X-API-KEY"].FirstOrDefault();
        //         var expectedApiKey = "my-secret-key"; // Replace with your actual key
        //
        //         if (apiKey != expectedApiKey)
        //         {
        //             context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //             await context.Response.WriteAsync("Unauthorized: Invalid API Key");
        //             return;
        //         }
        //     }
        //     await next();
        // });

// ✅ Database Health Check
        app.MapHealthChecks("/health-db", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("database"), // Only DB check
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description
                    }),
                    totalDuration = report.TotalDuration.TotalMilliseconds
                };
                await context.Response.WriteAsJsonAsync(result);
            }
        });
        
        // ✅ Secure Health Check UI with Basic Authentication
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/health-ui"))
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

                if (authHeader == null || !authHeader.StartsWith("Basic "))
                {
                    context.Response.Headers["WWW-Authenticate"] = "Basic";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();
                var decodedUsernamePassword = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));
                var usernamePassword = decodedUsernamePassword.Split(':', 2);

                var expectedUsername = "admin";  // Change in production
                var expectedPassword = "password"; // Change in production

                if (usernamePassword[0] != expectedUsername || usernamePassword[1] != expectedPassword)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }
            await next();
        });

// ✅ Map Health Check UI (Protected)
        app.UseHealthChecksUI(config => config.UIPath = "/health-ui");
    }
}