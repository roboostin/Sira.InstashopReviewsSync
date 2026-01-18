namespace API.Shared.Helpers
{
    public class HttpClientHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task HitUrlAsync(string url)
        {
            //var url = "https://example.com/api/endpoint"; // Replace with your actual URL

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Successfully hit {url} at {DateTime.UtcNow}");
                }
                else
                {
                    Console.WriteLine($"Failed to hit {url}. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hitting {url}: {ex.Message}");
            }
        }
    }
    }
