using API.Helpers;
using DotNetCore.CAP;

namespace API.Infrastructure.MessageBroker.Attributes
{
    public class CapDirectSubscribe : CapSubscribeAttribute
    {
        public CapDirectSubscribe() : base(ConfigurationHelper.GetInstashopMessageBrokerRoutingKey())
        {
            Group = ConfigurationHelper.GetInstashopMessageBrokerQueue();
        }
    }
}
