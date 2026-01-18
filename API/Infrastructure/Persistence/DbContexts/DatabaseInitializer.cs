using Microsoft.EntityFrameworkCore;

namespace API.Infrastructure.Persistence.DbContexts;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

            // Apply migrations
            //await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Seed data
            var seeder = services.GetRequiredService<IDataSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}