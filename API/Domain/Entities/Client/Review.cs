using API.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace API.Domain.Entities.Client;

[Table(name: "Review", Schema = "Client")]
public class Review : BaseEntity
{
    public SourceType Source { get; set; }
    //public long? LocationQuestionID { get; set; }
    public int Rate { get; set; }
    public string Feedback { get; set; }
    public string ReviewerName { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScrapedAt { get; set; }

    [Column(TypeName = "Date")]
    public DateOnly ReviewDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public long? LocationID { get; set; }
    public string LocationName { get; set; }
    public string Sentiment { get; set; }
    public bool IsProcessed { get; set; } = false;
 
    public virtual Location Location { get; set; }

}

public class CustomerNameHistory
{
    public string Note { get; set; }
    public DateTime? UpdatedAt { get; set; }
}