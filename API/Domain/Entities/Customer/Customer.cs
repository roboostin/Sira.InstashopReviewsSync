using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Customer;
[Table(name: "Customer", Schema = "Customer")]
public class Customer : BaseEntity
{
    public string Name { get; set; }
    public string Mobile { get; set; }
    
}