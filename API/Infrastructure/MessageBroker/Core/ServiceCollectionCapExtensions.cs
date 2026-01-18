using API.Helpers;
using API.Infrastructure.Persistence.DbContexts;

namespace API.Infrastructure.MessageBroker.Core;

public static class ServiceCollectionCapExtensions
{
    public static void ConfigureMessageBroker(this WebApplicationBuilder builder)
    {
        // Register CAP subscriber service - required for CAP to discover and invoke it
        builder.Services.AddScoped<MessageBrokerService, MessageBrokerService>();
        builder.Services.AddScoped<IEventPublisher, EventPublisher>();

        builder.Services.RegisterCapInterceptors();
        
        var rabbitConfig = ConfigurationHelper.GetRabbitMQCredentials();

        builder.Services.AddCap(x =>
            {
                x.UsePostgreSql(builder.Configuration.GetConnectionString("DefaultConnection")!);
                x.UseEntityFramework<ApplicationDbContext>();
                x.UseRabbitMQ(opt =>
                {
                    opt.HostName = rabbitConfig.host;
                    opt.Port = 5672;
                    opt.UserName = rabbitConfig.username;
                    opt.Password = rabbitConfig.password;
                    opt.ExchangeName = "sira.sync";
                });

                x.UseDashboard();
            })
            .AddSubscribeFilter<EventBusInterceptorFilter>();
    }
}