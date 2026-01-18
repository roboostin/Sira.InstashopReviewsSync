using API.Helpers;
using API.Infrastructure.MessageBroker;
using DotNetCore.CAP;

namespace API.Infrastructure.MessageBroker.Attributes;

public class CapFanoutSubscribeAttribute : CapSubscribeAttribute
{
    public CapFanoutSubscribeAttribute() : base(Constants.MessageBrokerFanoutRoutingKey)
    {
        Group = ConfigurationHelper.GetMessageBrokerQueue();
    }
}