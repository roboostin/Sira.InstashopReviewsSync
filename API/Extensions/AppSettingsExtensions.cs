using Newtonsoft.Json;

namespace API.Extensions;

public static class AppSettingsExtensions
{
    public static void ReadAppSettings(this WebApplicationBuilder webApplicationBuilder)
{
    var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config");

    webApplicationBuilder.Configuration
        .SetBasePath(configPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    
        // Load settings from organized folders
        .AddJsonFile($"Logging/appsettings.logging.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"Serilog/appsettings.serilog.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"Database/appsettings.database.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"Hangfire/appsettings.hangfire.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"HealthChecks/appsettings.healthchecks.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"Redis/appsettings.redis.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"RateLimiting/appsettings.rate-limiting.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"MessageBroker/appsettings.MessageBroker.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"MiniProfiler/appsettings.MiniProfiler.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"MiniProfiler/appsettings.MiniProfiler.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)

        .AddEnvironmentVariables();
    // Build configuration
     var configuration = webApplicationBuilder.Configuration;
    // // ✅ Print final merged configuration in JSON format
    Console.WriteLine("==== Final Merged Configuration ====");
    var finalConfig = configuration.AsEnumerable()
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    
    Console.WriteLine(JsonConvert.SerializeObject(finalConfig, Formatting.Indented));
    Console.WriteLine("====================================");
}

}