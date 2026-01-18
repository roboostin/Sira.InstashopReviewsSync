using API.Application.Filters;
using API.Application.Interfaces;
using API.Domain.UnitOfWork;
using API.EndPoints;
using API.Extensions;
using API.Helpers;
using API.Infrastructure.Mapping;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.Persistence.DbContexts;
using API.Middlewares;
using API.Shared.Models;
using FluentValidation;
using IdGen;
using Microsoft.IdentityModel.Logging;
using QuestPDF.Infrastructure;
using Serilog;
using System.Reflection;

try
{
    var builder = WebApplication.CreateBuilder(args);
    QuestPDF.Settings.License = LicenseType.Community;

    Log.Information("Starting Read App Settings");

    builder.ReadAppSettings();
    Log.Information("End Read App Settings");

    ConfigurationHelper.Initialize(builder.Configuration);

    builder.ConfigureSerilog();
    Log.Information("Starting APP");

    builder.Services.ConfigureServices(builder.Configuration);
    builder.Services.ConfigurePersistence(builder.Configuration, builder.Environment);
    builder.Services.ConfigureResponseCompression(builder.Configuration);
    builder.Services.ConfigureHttpClient();

    builder.Services.ConfigureHangfire(builder.Configuration);

    // Register UserService
    builder.Services.AddScoped<UserState>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ICancellationTokenProvider, CancellationTokenProvider>();
    builder.Services.AddScoped<RequestHandlerBaseParameters>();
    builder.Services.AddScoped<TransactionMiddleware>();
    builder.Services.AddScoped<UserStateInitializerMiddleware>();
    builder.Services.AddScoped<CaptureCancellationAndTimeZoneFilter>();
    builder.Services.AddSingleton(new IdGenerator(0));


    builder.Services.ConfigureCors(builder.Configuration);
    IdentityModelEventSource.ShowPII = true;
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<DateTimeInterceptor>();
    builder.ConfigureHybridCache();
    builder.ConfigureRateLimiting();
    builder.Services.ConfigureMiniProfiler(builder.Configuration);

    builder.ConfigureMessageBroker();
    builder.Services.AddMappingService();
    
    // Register background service for processing non-processed reviews
    builder.Services.AddHostedService<API.Application.Services.ProcessNonProcessedReviewsBackgroundService>();
    
    var app = builder.Build();

    app.UseHangfireDashboard(app.Configuration);
    app.UseMiniProfiler();
    app.UseMiniProfilerDashboard(app.Configuration);

    await app.Services.InitializeDatabaseAsync();
    app.EnableCors();
    app.UseAuthentication();
    app.UseAuthorization();
    
    var globalGroup = app.MapGroup("")
        .AddEndpointFilter<CaptureCancellationAndTimeZoneFilter>();
    // Auto-register endpoints
    var endpointDefinitions = typeof(Program).Assembly
        .GetTypes()
        .Where(t => typeof(EndpointDefinition).IsAssignableFrom(t) && !t.IsAbstract)
        .Select(Activator.CreateInstance)
        .Cast<EndpointDefinition>();

    foreach (var endpoint in endpointDefinitions)
    {
        endpoint.RegisterEndpoints(globalGroup);
    }

    app.UseRateLimiter();
    app.EnableResponseCompression();
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<RequestTimeoutMiddleware>();
    app.UseMiddleware<SlowRequestLoggingMiddleware>();
    app.UseMiddleware<UserStateInitializerMiddleware>();
    app.UseMiddleware<TransactionMiddleware>();

    // Use Global Exception Handling Middleware

    app.UseSerilogRequestLogging();
    app.ScheduleHangfireJobs(app.Configuration);



    app.ConfigureEndpoints();
    //app.UseHealthChecks();

    app.Run();

    Log.Information("APP STARTED");



}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    Console.WriteLine(exception.ToString(), "Application terminated unexpectedly");

}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    Log.Error("Shut down complete");
    Console.WriteLine("Shut down complete");

    Log.CloseAndFlush();
}