using API.Domain.Entities;
using API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace API.Infrastructure.Persistence.DbContexts;

public interface IDataSeeder
{
    Task SeedAsync();
}

public class DataSeeder : IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();

        // Execute Google tables creation script
        await ExecuteGoogleTablesScriptAsync();
    }

    private async Task ExecuteGoogleTablesScriptAsync()
    {
        try
        {
            // Try multiple paths to find the script
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "Database", "Scripts", "CreateGoogleTables.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "SiraApp", "API", "Database", "Scripts", "CreateGoogleTables.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "Scripts", "CreateGoogleTables.sql"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SiraApp", "API", "Database", "Scripts", "CreateGoogleTables.sql")
            };

            string? scriptPath = null;
            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    scriptPath = path;
                    break;
                }
            }

            if (scriptPath != null)
            {
                var scriptContent = await File.ReadAllTextAsync(scriptPath);
                // Split script by semicolons and execute each statement separately
                var statements = scriptContent
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("--"))
                    .ToList();

                foreach (var statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        await _context.Database.ExecuteSqlRawAsync(statement);
                    }
                }

                _logger.LogInformation("Google tables script executed successfully from: {Path}", scriptPath);
            }
            else
            {
                _logger.LogWarning("Google tables script not found in any of the expected paths. Please run the script manually.");
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - script might already be executed
            _logger.LogWarning(ex, "Error executing Google tables script. Tables might already exist.");
        }
    }
}