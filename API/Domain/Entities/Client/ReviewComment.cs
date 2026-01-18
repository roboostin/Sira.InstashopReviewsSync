using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Client
{
    [Table(name: "ReviewComment", Schema = "Client")]
    public class ReviewComment : BaseEntity
    {
        public string Comment { get; set; }
        public long ReviewID { get; set; }
        public string UserName { get; set; }
    }
}
