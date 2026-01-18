namespace API.Helpers;

public static class ConfigurationHelper
{
    private static IConfiguration _configuration;

    public static void Initialize(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static string GetConfigurationValue(string key)
    {
        return _configuration[key];
    }

    public static T GetSection<T>(string key)
    {
        var section = _configuration.GetSection(key);
        return section.Exists() ? section.Get<T>() : default;
    }

    public static string GetConnectionString(string name = "Default")
    {
        return GetConfigurationValue($"ConnectionStrings:{name}");
    }

    public static string GetApplicationName()
    {
        return GetConfigurationValue("Serilog:Properties:ApplicationName");
    }

    public static string GetAuthorizationConnectionString()
    {
        var connectionString = GetConfigurationValue("ConnectionStrings:Authorization");

        if (string.IsNullOrEmpty(connectionString))
            connectionString = GetConfigurationValue("ConnectionStrings:Default");

        return connectionString;
    }
    public static int GetDBCommandTimeOut()
    {
        int.TryParse(GetConfigurationValue("Database:CommandTimeOut"), out int timeout);
        return timeout != 0 ? timeout : Constants.DBCommandTimeout;
    }

 



    public static string GetURL()
    {
        try
        {
            return GetConfigurationValue("Environment:URL");
        }
        catch (Exception ex)
        {
            return "https://api.tayar.app";
        }
    }
    public static string GetCompanyName()
    {
        try
        {
            return GetConfigurationValue("Environment:Name");
        }
        catch (Exception ex)
        {
            return "Roboost";
        }
    }


 


    public static bool IsEnvironment(string name)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(name.Trim(), environment.Trim(), StringComparison.CurrentCultureIgnoreCase);

    }


    public static bool AllowRedis()
    {
        var value = GetConfigurationValue("RedisSettings:Enabled");

        return Convert.ToBoolean(String.IsNullOrEmpty(value) ? false : value);
    }
    
    public static string GetMessageBrokerRoutingKey()
    {
        return GetConfigurationValue("MessageBroker:RoutingKey");
    }

    public static string GetTalabatMessageBrokerRoutingKey()
    {
        return $"{GetConfigurationValue("MessageBroker:RoutingKey")}.talabat";
    }

    public static string GetTalabatMessageBrokerACKRoutingKey()
    {
        return $"{GetConfigurationValue("MessageBroker:RoutingKey")}.talabat.review.ack";
    }

    public static string GetMessageBrokerRoutingKeyPrefix()
    {
        return GetConfigurationValue("MessageBroker:RoutingKeyPrefix");
    }
   
    public static string GetMessageBrokerQueue()
    {
        return GetConfigurationValue("MessageBroker:Queue");
    }

    public static string GetTalabatMessageBrokerQueue()
    {
        return $"{GetConfigurationValue("MessageBroker:Queue")}.talabat";
    }

    public static (string host, string username, string password) GetRabbitMQCredentials()
    {
        return (GetConfigurationValue("RabbitMQ:Host"), GetConfigurationValue("RabbitMQ:Username"), GetConfigurationValue("RabbitMQ:Password"));
    }
}