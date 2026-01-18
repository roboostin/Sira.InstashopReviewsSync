using API.Infrastructure.MessageBroker.Core;
using DotNetCore.CAP.Filter;
using Newtonsoft.Json;

namespace API.Infrastructure.MessageBroker.Interceptors;

public class ErrorLoggingEventBusInterceptor(ILogger<ErrorLoggingEventBusInterceptor> logger) : IEventBusInterceptor
{
    public double ExecutionOrder => 1;
    
    public Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        var ex = context.Exception;
        var deliveredMessage = context.DeliverMessage; 
        var msg = $"ErrorLoggingEventBusInterceptor:{Environment.NewLine}{ex.Message} - {ex.InnerException?.Message}{Environment.NewLine}DeliveredMessage: {JsonConvert.SerializeObject(deliveredMessage)}";

        logger.LogError(ex, msg);    
        
        return Task.CompletedTask;
    }
}