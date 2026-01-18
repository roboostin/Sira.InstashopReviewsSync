using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using UUIDNext;

namespace API.Domain.Entities
{
    public class RedisBaseModel
    {
        [MaxLength(36)]
        public long ID { get; set; }
        [MaxLength(36)]
        public long CompanyID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual DateTime? TTL { get; set; }
    }
}
