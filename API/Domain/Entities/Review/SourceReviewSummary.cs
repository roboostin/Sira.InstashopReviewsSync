using System.ComponentModel.DataAnnotations.Schema;
using API.Domain.Enums;

namespace API.Domain.Entities.Review;

[Table(name: "SourceReviewSummary", Schema = "Review")]
public class SourceReviewSummary : BaseEntity
{
    public double? TalabatAvgRating { get; set; }
    public int? TalabatTotalResponses { get; set; }
    public DateOnly Date { get; set; }
    public long LocationID { get; set; }
    public double AvgRate { get; set; }
    public long TotalResponses { get; set; }
}