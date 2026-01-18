using API.Infrastructure.MessageBroker.Core;
using DotNetCore.CAP.Filter;

namespace API.Infrastructure.MessageBroker.Interceptors;

public class EventPublisherEventBusInterceptor(IEventPublisher publisher)
    : IEventBusInterceptor
{
    public double ExecutionOrder => 4;

    public Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        
        publisher.Publish();

        return Task.CompletedTask;
    }

    public Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        return Task.CompletedTask;
    }
}