namespace API.Config.RateLimiting;

public class RateLimitingSettings
{
    //public bool RedisEnabled { get; set; } = false;
    // public string RedisConnectionString { get; set; } = string.Empty;
    // public string RedisInstanceName { get; set; } = "RateLimiter:";
    // public int DefaultRequestsPerSecond { get; set; } = 2;
    public int DefaultTimeWindowInSeconds { get; set; } = 1;
}