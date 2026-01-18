using System.Reflection;

namespace API.Infrastructure.MessageBroker.Core;

public static class MessageBrokerExtension
{
    public static async Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
    {
        dynamic awaitable = @this.Invoke(obj, parameters);
        await awaitable;
        awaitable.GetAwaiter().GetResult();
    }
}