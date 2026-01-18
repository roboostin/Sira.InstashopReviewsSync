using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class EditedLocationMessage : BaseMessage
    {
        public long LocationID { get; set; }
        public string LocationName { get; set; }
    }
}
