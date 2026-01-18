using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Core;

public interface IBaseConsumer<T> where T : BaseMessage
{
    Task Consume(T message);
}
