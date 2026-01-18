using Hangfire.Dashboard;

namespace API.Extensions
{
   // Custom Authentication Filter for Hangfire
public class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireDashboardAuthFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        var authHeader = httpContext.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var credentials = decodedCredentials.Split(':');

            if (credentials.Length == 2)
            {
                var username = credentials[0];
                var password = credentials[1];

                return username == _username && password == _password;
            }
        }

        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        httpContext.Response.StatusCode = 401; // Unauthorized
        return false;
    }
}
}