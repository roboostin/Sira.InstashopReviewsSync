using API.Shared.Models;
using DotNetCore.CAP;
using Newtonsoft.Json;

namespace API.Infrastructure.MessageBroker.Core;

public class EventPublisher(UserState userState, ICapPublisher capPublisher, ILogger<EventPublisher> logger) : IEventPublisher
{
    private List<MessageBrokerEvent> _events = [];
    
    public void AddEvent(MessageBrokerEvent baseEvent)
    {
        baseEvent.Message.Username = userState.Username;
        baseEvent.Message.RoleID = baseEvent.Message.RoleID;

        _events?.Add(baseEvent);
        logger.LogDebug("Event added. Total events count: {EventCount}", _events?.Count ?? 0);
    }

    public void Publish()
    {
        PublishEvents();
        _events?.Clear();
    }

    public void PublishEvents()
    {
        
        // Log calling context and EventPublisher instance ID for diagnostics
        var stackTrace = new System.Diagnostics.StackTrace();
        var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
        var callingInfo = callingMethod != null
            ? $"{callingMethod.DeclaringType?.FullName}.{callingMethod.Name}"
            : "Unknown";
        

        if (_events is null or { Count: 0 })
            return;

        var instanceId = this.GetHashCode();

        logger.LogInformation("PublishEvents called from: {CallingInfo}. EventPublisher instance ID: {InstanceId}.", callingInfo, instanceId);

        var eventCount = _events.Count;
        
        var events = new MessageBrokerEvent[_events.Count] ;
        _events.CopyTo(events, 0);
            
        foreach (var @event in events)
        {
            foreach (var target in @event.Targets)
            {
                var headers = new Dictionary<string, string>();
                    
                if (!string.IsNullOrEmpty(userState.Username))
                    headers.Add("Username", userState.Username);
                    
                headers.Add("UserID", userState.UserID.ToString());
                    
                headers.Add("CompanyID", userState.CompanyID.ToString());
                    
                if (userState.RoleID != 0)
                    headers.Add("RoleID", ((int)userState.RoleID).ToString());
                    
                @event.Message.MessageType = @event.Message.GetType().Name;
                capPublisher.Publish(target, JsonConvert.SerializeObject(@event.Message), headers);
                logger.LogDebug("Published event {MessageType} to target {Target}", @event.Message.MessageType, target);
            }
        }
        
        logger.LogInformation("Successfully published {EventCount} events", eventCount);
    }
}
