using DotNetCore.CAP.Filter;

namespace API.Infrastructure.MessageBroker.Core;

public class EventBusInterceptorFilter : ISubscribeFilter
{
    private readonly IEnumerable<IEventBusInterceptor> _ascendingSortedEventBusInterceptors;
    private readonly IEnumerable<IEventBusInterceptor> _descendingSortedEventBusInterceptors;

    public EventBusInterceptorFilter(IServiceProvider serviceProvider)
    {
        var eventBusInterceptors = serviceProvider.GetServices<IEventBusInterceptor>();

        var eventBusInterceptorsList = eventBusInterceptors.ToList();
        
        _ascendingSortedEventBusInterceptors = eventBusInterceptorsList.OrderBy(x => x.ExecutionOrder);
        
        _descendingSortedEventBusInterceptors = eventBusInterceptorsList.OrderByDescending(x => x.ExecutionOrder);
    }

    public async Task OnSubscribeExecutingAsync(ExecutingContext context)
    {
        foreach (var eventBusInterceptor in _ascendingSortedEventBusInterceptors)
        {
            await eventBusInterceptor.OnSubscribeExecutingAsync(context).ConfigureAwait(false);
        }
    }
    
    public async Task OnSubscribeExecutedAsync(ExecutedContext context)
    {
        foreach (var eventBusInterceptor in _descendingSortedEventBusInterceptors)
        {
            await eventBusInterceptor.OnSubscribeExecutedAsync(context).ConfigureAwait(false);
        }
    } 
    
    public async Task OnSubscribeExceptionAsync(ExceptionContext context)
    {
        foreach (var eventBusInterceptor in _descendingSortedEventBusInterceptors)
        {
            await eventBusInterceptor.OnSubscribeExceptionAsync(context).ConfigureAwait(false);
        }
    }
}