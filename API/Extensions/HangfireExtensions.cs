using API.Config.Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Redis.StackExchange;

namespace API.Extensions;

// API/Extensions/HangfireExtensions.cs
using Hangfire;
using Hangfire.SqlServer;



public static class HangfireExtensions
{
    public static void ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireSettings = configuration.GetSection("HangfireSettings").Get<HangfireSettings>();

        if (hangfireSettings?.Enabled == true)
        {
            var hangfireConnectionString = hangfireSettings.ConnectionString;
            Console.WriteLine($"hangfireConnectionString : {hangfireConnectionString}");
            if (string.IsNullOrEmpty(hangfireConnectionString))
            {
                throw new Exception("Hangfire: Connection string is missing. Ensure it's set in appsettings.json.");
            }

            UsePostgres(hangfireConnectionString);


            services.AddHangfireServer();
        }

        void UsePostgres(string hangfireConnectionString)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(hangfireConnectionString, new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    QueuePollInterval = TimeSpan.FromMilliseconds(200),
                    InvisibilityTimeout = TimeSpan.FromMinutes(5), 
                    DistributedLockTimeout = TimeSpan.FromMinutes(5), 
                    PrepareSchemaIfNecessary = true,
                    EnableTransactionScopeEnlistment = true
                }));
        }
        // For MS SQL Server
        void UseSqlServer(string hangfireConnectionString)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
                {
                    QueuePollInterval = TimeSpan.Zero,
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
        }

// For Redis
        void UseRedis(string hangfireConnectionString)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseRedisStorage(hangfireConnectionString, new RedisStorageOptions
                {
                    Prefix = "hangfire:",
                    Db = 0,
                    SucceededListSize = 10000,
                    DeletedListSize = 1000,
                    InvisibilityTimeout = TimeSpan.FromMinutes(30)
                }));
        }
    }

    public static void UseHangfireDashboard(this IApplicationBuilder app, IConfiguration configuration)
    { 
        var hangfireSettings =configuration.GetSection("HangfireSettings").Get<HangfireSettings>();

        if (hangfireSettings?.Enabled == true)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthFilter("admin", "Sira321@") }
            });
        }

    }
}