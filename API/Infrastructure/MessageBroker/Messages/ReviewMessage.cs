using API.Domain.Enums;
using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class ReviewMessage : BaseMessage
    {
        public long ID { get; set; }
        public long LocationID { get; set; }
        public string LocationName { get; set; }
        public string Feedback { get; set; }
        public string ReviewerName { get; set; }
        public DateTime PublishedAt { get; set; }
        public DateOnly ReviewDate { get; set; }
        public SourceType Source { get; set; } = SourceType.Talabat;
        public string Sentiment { get; set; }
        public long? ChannelID { get; set; }
        public int Rate { get; set; }
    }
}
