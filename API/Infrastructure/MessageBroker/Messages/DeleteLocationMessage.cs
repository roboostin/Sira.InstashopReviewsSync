using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class DeleteLocationMessage : BaseMessage
    {
        public long LocationID { get; set; }
    }
}

