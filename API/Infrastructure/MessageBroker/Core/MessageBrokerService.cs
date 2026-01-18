using API.Infrastructure.MessageBroker.Attributes;
using API.Shared.Models;
using Newtonsoft.Json;
using DotNetCore.CAP;
using MediatR;

namespace API.Infrastructure.MessageBroker.Core;

public class MessageBrokerService(IMediator mediator, ILogger<MessageBrokerService> logger, IConfiguration configuration)
    : ICapSubscribe
{
    private readonly string messagesNamespace = "API.Infrastructure.MessageBroker.Messages";
    private readonly string consumersNamespace = "API.Infrastructure.MessageBroker.Consumers";
    

    public void GetMessageBrokerStatus()
    {
        // This method is intentionally left blank.
        // It can be used to check if the MessageBrokerService is registered and operational.
        
    }

    [ACKSubscribe]
    [CapDirectSubscribe]
    //[CapFanoutSubscribe]
    public async Task Consume(string message)
    {
        if (bool.TryParse(configuration["MessageBroker:Enabled"], out var isEnabled) && !isEnabled)
            return;

        try
        {
            var baseMessage = GetMessage(message);

            if (baseMessage == null)
                return;

            await InvokeConsumer(baseMessage);
        }
        catch (Exception ex)
        {
            var msg = $"\nMessageBroker Error: {ex.Message}\n========================================";

            logger.LogError(ex, msg);

            throw;
        }

    }

    private BaseMessage GetMessage(string body)
    {
        var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(body);
        var typeValue = jsonObject["MessageType"].ToString();

        Type type = Type.GetType($"{messagesNamespace}.{typeValue}");
        BaseMessage message = default;

        if (type != null)
        {
            message = JsonConvert.DeserializeObject(body, type) as BaseMessage;
            message.MessageType = message.MessageType.Replace("Message", "Consumer");
        }

        return message;
    }

    private async Task InvokeConsumer(BaseMessage baseMessage)
    {
        var consumerPath = $"{consumersNamespace}.{baseMessage.MessageType}";

        var consumerType = Type.GetType(consumerPath);

        if (consumerType == null)
            return;

        var consumer = Activator.CreateInstance(consumerType, mediator);
        //await consumer.Consume(baseMessage as SendTaskToAnalyticsMessage);
        var method = consumerType.GetMethod("Consume");

        await method.InvokeAsync(consumer, new object[] { baseMessage });
        //Task.Delay(5000);
    }
}