using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Client;

[Table(name:"Groups", Schema = "Client")]
public class Group : BaseEntity
{
    public string Name { get; set; }
    public virtual ICollection<Location> Locations { get; set; }
}