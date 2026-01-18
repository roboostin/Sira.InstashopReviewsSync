using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Core;

public interface IEventPublisher
{
    public void AddEvent(MessageBrokerEvent baseEvent);
    void Publish();
    void PublishEvents();
}