using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class CreatedLocationMessage : BaseMessage
    {
        public long LocationID { get; set; }
        public List<int>? TalabatLocationsIDs { get; set; }
        public string LocationName { get; set; }
    }
}
