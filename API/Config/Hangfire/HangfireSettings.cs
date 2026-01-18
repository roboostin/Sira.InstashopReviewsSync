namespace API.Config.Hangfire;

public class HangfireSettings
{
    public bool Enabled { get; set; }
    public HangfireStorageType StorageType { get; set; }
    public string ConnectionString { get; set; }
}

public enum HangfireStorageType
{
    PostgreSql,
    SqlServer,
    Redis
}