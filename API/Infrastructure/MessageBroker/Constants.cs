using API.Helpers;

namespace API.Infrastructure.MessageBroker;

public static class Constants
{
    public static string MessageBrokerRoutingKey => ConfigurationHelper.GetMessageBrokerRoutingKey();
    public static string MessageBrokerAggregatorRoutingKey => $"{ConfigurationHelper.GetMessageBrokerRoutingKey()}.review.published";
    public static string MessageBrokerReviewACKRoutingKey => $"{ConfigurationHelper.GetMessageBrokerRoutingKey()}.instashop.review.ack";
    public static string MessageBrokerFanoutRoutingKey => $"{ConfigurationHelper.GetMessageBrokerRoutingKeyPrefix()}.fanout";
}