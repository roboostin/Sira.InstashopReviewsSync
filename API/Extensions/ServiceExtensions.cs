
using API.Application.Services.ReviewaDataFilter;

namespace API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; 
                
                // options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.WriteAsString;
            });
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null; // Keeps PascalCase
        });
        services.AddOpenApi(options =>
        {
        });

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddScoped<IScraperHttpClientService, ScraperHttpClientService>();
        services.AddScoped<IReviewDateFilterService, ReviewDateFilterService>();

        return services;
    }

}