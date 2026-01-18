namespace API.Shared.Models;

public class MessageBrokerEvent(BaseMessage message, params string[] targets)
{
    public BaseMessage Message { get; init; } = message;
    public string[] Targets { get; init; } = targets;
}
