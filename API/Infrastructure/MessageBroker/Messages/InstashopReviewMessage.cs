using API.Domain.Entities.Client;
using API.Domain.Enums;
using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class InstashopReviewMessage : BaseMessage
    {
        public string InstashopClientId { get; set; }
        public long LocationID { get; set; }
        public string LocationName { get; set; }
        public string Comment { get; set; }
        public int ProductAccuracy { get; set; }
        public int DeliverySpeed { get; set; }
        public string CreatedAt { get; set; }
        public string Area { get; set; }
        public long CompanyID { get; set; }
    }
}
