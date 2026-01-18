using API.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Client;

[Table(name: "LocationSource", Schema = "Client")]
public class LocationSource : BaseEntity
{
    [ForeignKey("Location")]
    public long LocationID { get; set; }

    public SourceType Source { get; set; }

    [MaxLength(500)]
    public string Link { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Location Location { get; set; }
}

