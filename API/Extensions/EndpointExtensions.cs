using Scalar.AspNetCore;

namespace API.Extensions;

public static class EndpointExtensions
{
    public static void ConfigureEndpoints(this WebApplication app)
    {
        app.MapGet("/", async context =>
        {
            context.Response.Redirect("/scalar/v1", true);
        });
        
       
        app.MapOpenApi(); // Custom OpenAPI endpoint mapping
        app.MapScalarApiReference(); // Scalar API reference mapping
        
        app.MapControllers();
    }
    
    // public static RouteHandlerBuilder RequiresFeature(this RouteHandlerBuilder builder,params string[] features)
    // {
    //      builder.RequireAuthorization(policy => 
    //         policy.Requirements.Add(new FeatureRequirement(features)));
    //     
    //     // Add an endpoint filter to handle unauthenticated requests better
    //     return builder.AddEndpointFilter(async (context, next) =>
    //     {
    //         var httpContext = context.HttpContext;
    //         
    //         if (!httpContext.User.Identity?.IsAuthenticated ?? true)
    //         {
    //             return Results.Unauthorized();
    //         }
    //         
    //         return await next(context);
    //     });
    // }
    //
}