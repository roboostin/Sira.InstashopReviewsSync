using API.Helpers;
using DotNetCore.CAP;

namespace API.Infrastructure.MessageBroker.Attributes;

public class ACKSubscribe : CapSubscribeAttribute
{
    public ACKSubscribe() : base(ConfigurationHelper.GetInstashopMessageBrokerACKRoutingKey())
    {
        Group = ConfigurationHelper.GetInstashopMessageBrokerQueue();
    }
}