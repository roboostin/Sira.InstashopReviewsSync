using API.Domain.Enums;

namespace API.Shared.Models;

public class BaseMessage
{
    public string Username { get; set; }
    public long UserID { set; get; }
    public ApplicationRole RoleID { get; set; }
    public long CompanyID { set; get; }
    public string MessageID { get; set; } = Guid.NewGuid().ToString();
    public string MessageType { get; set; }
    public string PublisherRoutingKey { get; set; }
    public string ACKRoutingKey { get; set; }
    public DateTime MessageCreatedDate { get; set; } = DateTime.UtcNow;
}
