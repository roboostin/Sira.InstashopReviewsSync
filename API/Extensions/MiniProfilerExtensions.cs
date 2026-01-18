using Microsoft.Extensions.Caching.Memory;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace API.Extensions;

public static class MiniProfilerExtensions
{
    public static void ConfigureMiniProfiler(this IServiceCollection services, IConfiguration configuration)
    {
        var miniProfilerSettings = configuration.GetSection("MiniProfilerSettings").Get<MiniProfilerSettings>();

        if (miniProfilerSettings?.Enabled == true)
        {
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
                options.EnableDebugMode = miniProfilerSettings.DebugMode;

                ConfigureStorage(options, miniProfilerSettings, services);

                options.TrackConnectionOpenClose = true;
                options.EnableMvcFilterProfiling = true;
                options.EnableMvcViewProfiling = true;
                //options.EnableMemoryProfiling = miniProfilerSettings.EnableMemoryProfiling;
                //options.EnableGarbageCollectionTracker = miniProfilerSettings.EnableGCTracking;
            }).AddEntityFramework();
        }
    }

    private static void ConfigureStorage(MiniProfilerOptions options, MiniProfilerSettings settings, IServiceCollection services)
    {
        switch (settings.StorageProvider?.ToLower())
        {
            //case "postgresql":
            //    options.Storage = new PostgreSqlStorage(settings.ConnectionString);
            //    break;

            case "memory":
            default:
                var serviceProvider = services.BuildServiceProvider();
                var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
                options.Storage = new MemoryCacheStorage(memoryCache, TimeSpan.FromMinutes(30)); break;
        }
    }

    public static void UseMiniProfilerDashboard(this IApplicationBuilder app, IConfiguration configuration)
    {
        var miniProfilerSettings = configuration.GetSection("MiniProfilerSettings").Get<MiniProfilerSettings>();

        if (miniProfilerSettings?.Enabled == true)
        {
            // This injects the MiniProfiler widget into your pages
            app.UseMiniProfiler();

            // Configure the dashboard UI routes
            app.Map("/profiler", subApp =>
            {
                // Optional authentication
                if (miniProfilerSettings.RequireAuth)
                {
                    subApp.Use(async (context, next) =>
                    {
                        if (!context.User.Identity.IsAuthenticated ||
                           !context.User.IsInRole("Admin"))
                        {
                            context.Response.StatusCode = 401;
                            return;
                        }
                        await next();
                    });
                }

                // This enables the MiniProfiler dashboard UI
                subApp.UseMiniProfiler();

                // Add a redirect from the base path to the results index
                subApp.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/profiler")
                    {
                        context.Response.Redirect("/profiler/results-index");
                        return;
                    }
                    await next();
                });
            });
        }
    }
}

public class MiniProfilerSettings
{
    public bool Enabled { get; set; } = false;
    public bool DebugMode { get; set; } = false;
    public bool EnableMemoryProfiling { get; set; } = false;
    public bool EnableGCTracking { get; set; } = false;
    public bool RequireAuth { get; set; } = true;
    public string? StorageProvider { get; set; }
    public string? ConnectionString { get; set; }
}