using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class CreatedLocationMessage : BaseMessage
    {
        public long LocationID { get; set; }
        public string InstashopClientId { get; set; }
        public string LocationName { get; set; }
    }
}
