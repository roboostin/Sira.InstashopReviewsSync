using API.Application.Services;
using API.Infrastructure.Interceptors;
using API.Infrastructure.Persistence.DbContexts;
using API.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Extensions;
public static class PersistenceExtensions
{
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        services.AddScoped<CancellationInterceptor>();

        dataSourceBuilder.EnableDynamicJson(); // This method exists on NpgsqlDataSourceBuilder
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var cancellationInterceptor = serviceProvider.GetRequiredService<CancellationInterceptor>();

            // Use Npgsql for PostgreSQL
            options.UseNpgsql(dataSource);

            // Apply additional settings in Development environment
            if (environment.IsDevelopment())
            {
                // Enable query logging
                options.EnableSensitiveDataLogging(); // Logs parameter values (use carefully in production)
                options.EnableDetailedErrors(); // Provides more detailed error messages

                // Log generated SQL queries
                options.LogTo(Console.WriteLine); // Use a logger for more structured logging in production
            }

            // Apply a default behavior of no tracking
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.AddInterceptors(cancellationInterceptor);
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IAnalyticsRepositoryView<>), typeof(AnalyticsRepositoryView<>));
        services.AddScoped<IDataSeeder, DataSeeder>();

        return services;
    }
}
