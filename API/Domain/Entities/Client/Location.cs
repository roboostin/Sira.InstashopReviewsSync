using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Domain.Entities.Client;

namespace API.Domain.Entities
{
    [Table(name:"Location",Schema = "Client")]
    public class Location:BaseEntity
    {
        [MaxLength(100)]
        public string Name { get; set; }
        public List<int> TalabatLocationIDs { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastScrapeAttemptTime { get; set; }
        public DateTime? LastSuccessfulScrapeTime { get; set; }
    }
}
