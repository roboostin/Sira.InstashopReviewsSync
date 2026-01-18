using API.Domain.Enums;
using API.Shared.Models;

namespace API.Infrastructure.MessageBroker.Messages
{
    public class ReviewSummaryMessage : BaseMessage
    {
        public long LocationID { get; set; }
        public long CompanyID { get; set; }
        public double Rating { get; set; }
        public int TotalResponseCount { get; set; }
        public SourceType Source { get; set; } = SourceType.Talabat;
    }
}
