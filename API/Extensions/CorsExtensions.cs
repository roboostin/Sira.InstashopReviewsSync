namespace API.Extensions;

public static class CorsExtensions
{
    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        // Enable CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins",
                policy =>
                {
                    policy.WithOrigins("https://mc-v4.roboost.app") // Replace with your frontend domain
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            options.AddPolicy("AllowSpecificOrigins",
               policy =>
               {
                   policy.WithOrigins("http://localhost:4200") // Replace with your frontend domain
                       .AllowAnyMethod()
                       .AllowAnyHeader();
               });

            options.AddPolicy("AllowAllOrigins",
                policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    public static void EnableCors(this IApplicationBuilder app)
    {
        // Use CORS
        app.UseCors("AllowAllOrigins");
    }
}