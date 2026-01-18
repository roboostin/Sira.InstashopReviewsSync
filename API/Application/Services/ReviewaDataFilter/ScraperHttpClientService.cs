using API.Shared.Helpers;
using System.Text.Json;

namespace API.Application.Services.ReviewaDataFilter
{
    public class ScraperHttpClientService : IScraperHttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ScraperHttpClientService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ScraperHttpClientService(
            IHttpClientFactory httpClientFactory,
            ILogger<ScraperHttpClientService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse?> GetReviewsAsync<TResponse>(
            string baseUrl,
            string endpoint,
            List<QueryParam> queryParameters,
            CancellationToken cancellationToken = default) where TResponse : class
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                if (httpClient == null)
                {
                    _logger.LogError("Failed to create HttpClient from factory");
                    return null;
                }

                // Build the query string with null checks
                var queryString = string.Join("&", queryParameters
                    .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Key) && p.Value != null)
                    .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value ?? string.Empty)}"));

                if (string.IsNullOrWhiteSpace(queryString))
                {
                    _logger.LogInformation("Query string is empty, cannot make API call");
                    return null;
                }

                var fullUrl = $"{baseUrl}{endpoint}?{queryString}";

                _logger.LogDebug("Calling scraper API: {Url}", fullUrl);

                // Get raw JSON response with cancellation token
                var jsonResponse = await httpClient.GetStringAsync(fullUrl, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    _logger.LogInformation("Empty JSON response from scraper API");
                    return null;
                }

                _logger.LogDebug("Raw JSON response length: {Length} characters", jsonResponse.Length);

                // Deserialize using System.Text.Json
                var response = JsonSerializer.Deserialize<TResponse>(jsonResponse, _jsonOptions);

                return response;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogInformation(ex, "Request to scraper API was cancelled");
                return null;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling scraper API");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for scraper API response");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling scraper API");
                return null;
            }
        }
    }
}
