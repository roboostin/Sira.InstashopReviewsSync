using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class ReviewReceivedMessage : BaseMessage
    {
        public long ID { get; set; }
    }
}

