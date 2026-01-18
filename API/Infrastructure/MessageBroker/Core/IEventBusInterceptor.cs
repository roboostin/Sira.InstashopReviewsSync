using DotNetCore.CAP.Filter;

namespace API.Infrastructure.MessageBroker.Core;

public interface IEventBusInterceptor
{
    public double ExecutionOrder { get; }

    public Task OnSubscribeExecutingAsync(ExecutingContext context);

    public Task OnSubscribeExecutedAsync(ExecutedContext context);

    public Task OnSubscribeExceptionAsync(ExceptionContext context);
}