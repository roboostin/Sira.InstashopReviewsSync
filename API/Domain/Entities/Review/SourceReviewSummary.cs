using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Review;

[Table(name: "SourceReviewSummary", Schema = "Review")]
public class SourceReviewSummary : BaseEntity
{
    public double? InstashopAvgRating { get; set; }
    public int? InstashopTotalResponses { get; set; }
    public DateOnly Date { get; set; }
    public long LocationID { get; set; }
    public double AvgRate { get; set; }
    public long TotalResponses { get; set; }
}