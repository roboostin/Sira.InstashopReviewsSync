using API.Infrastructure.MessageBroker.Interceptors;

namespace API.Infrastructure.MessageBroker.Core;

public static class CapInterceptorsServiceRegisterer
{
    public static IServiceCollection RegisterCapInterceptors(this IServiceCollection services)
    {
        services.AddScoped<IEventBusInterceptor, ErrorLoggingEventBusInterceptor>();
        services.AddScoped<IEventBusInterceptor, TransactionEventBusInterceptor>();
        services.AddScoped<IEventBusInterceptor, UserStateInitializerEventBusInterceptor>();
        services.AddScoped<IEventBusInterceptor, EventPublisherEventBusInterceptor>();

        return services;
    }
}