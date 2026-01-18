using System.Security.Claims;
using System.Threading.RateLimiting;
using API.Config.RateLimiting;

namespace API.Extensions;

public static class RateLimitingExtensions
{
    public static void ConfigureRateLimiting(this WebApplicationBuilder builder)
    {
        var RateLimitingSettings = builder.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

        // if (RateLimitingSettings.RedisEnabled)
        // {
        //     var redisConnection = ConnectionMultiplexer.Connect(RateLimitingSettings.RedisConnectionString);
        //     builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
        // }

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy("custom", context =>
            {
                var endpoint = context.GetEndpoint();
                var metadata = endpoint?.Metadata.GetMetadata<RateLimitMetadata>();

                if (metadata != null)
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetUserKey(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = metadata.Requests,
                            Window = TimeSpan.FromSeconds(metadata.Seconds),
                            QueueLimit = 0
                        });
                }

                // Default fallback
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetUserKey(context),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 1,
                        Window = TimeSpan.FromSeconds(1),
                        QueueLimit = 0
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Rate limit exceeded",
                    message = "Too many requests. Please try again later."
                }, cancellationToken);
            };
        });
    }

    private static string GetUserKey(HttpContext context)
    {
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                     context.User?.Identity?.Name;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        return $"ip:{context.Connection.RemoteIpAddress}";
    }

    public static RouteHandlerBuilder WithRateLimit(this RouteHandlerBuilder builder, int requests, int seconds = 1)
    {
        return builder.RequireRateLimiting("custom")
                     .WithMetadata(new RateLimitMetadata(requests, 1));
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    public RateLimitAttribute(int requests, int seconds = 1)
    {
        Requests = requests;
        Seconds = seconds;
    }

    public int Requests { get; }
    public int Seconds { get; }
}

public class RateLimitMetadata
{
    public int Requests { get; }
    public int Seconds { get; }

    public RateLimitMetadata(int requests, int seconds)
    {
        Requests = requests;
        Seconds = seconds;
    }
}