using API.Domain.Entities.Client;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Domain.Entities
{
    [Table(name: "LocationChannel", Schema = "Client")]
    public class LocationChannel:BaseEntity
    {
        [ForeignKey("Channel")]
        public long ChannelID { get; set; }
        [ForeignKey("Location")]
        public long LocationID { get; set; }
        public bool IsQRCodeActive { get; set; } = false;
        public bool IsChannelActive { get; set; } = true;
        public string QRCodeText { get; set; }
        public virtual Channel Channel { get; set; }
        public virtual Location Location { get; set; }
    }
}
