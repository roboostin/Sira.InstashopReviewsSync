namespace API.Shared.Models;

/// <summary>
/// Demo message class for testing event publishing
/// </summary>
public class DemoMessage : BaseMessage
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

