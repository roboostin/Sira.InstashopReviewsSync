using Microsoft.AspNetCore.Authorization;

namespace API.Middlewares;

public class GlobalAuthorizationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // Get endpoint metadata and check for AllowAnonymous
        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            return await next(context); // Skip authentication check
        }

        // Ensure the user is authenticated
        var user = httpContext.User;
        if (user?.Identity is not { IsAuthenticated: true })
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}