using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities.Client
{
    [Table(name: "Channel", Schema = "Client")]
    public class Channel : BaseEntity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AllowedSendSMS { get; set; }
        public bool IsActive { get; set; }
        public long? SurveyID { get; set; }

        public virtual ICollection<LocationChannel> LocationChannels { get; set; }

    }
}
