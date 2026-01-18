using API.Helpers;

namespace API.Extensions;

public static class HttpClientExtensions
{
    public static void ConfigureHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("AIReplyClient", client =>
        {
            client.BaseAddress = new Uri(Constants.AIReviewReplyAPI);
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Add("User-Agent", "Sira-AI-Reply/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = true,
            UseCookies = true,
        });
    }
}