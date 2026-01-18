namespace API.Shared.Helpers;

using Newtonsoft.Json;
using RestSharp;
  public class RestSharpHelper
    {
        public static IRestRequest Request { get; set; }
        public static IRestResponse Response { get; set; }
       
        public static async Task<(RestRequest request, T response)> GetAsync<T>(string baseURL, string url, Dictionary<string, string> headers = null, List<QueryParam> queryParameters = null)
        {
            var client = new RestClient(baseURL);

            var request = new RestRequest(url);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    request.AddHeader(entry.Key, entry.Value);
                }
            }
            if (queryParameters != null)
            {
                foreach (var entry in queryParameters)
                {
                    request.AddQueryParameter(entry.Key, entry.Value);
                }
            }
        //var e= request.ToString();
           
        // Build and print the full URL for tracing
        var fullUri = client.BuildUri(request);
        Console.WriteLine($"[RestSharp] Full URL: {fullUri}");
           
        // Get raw response first to print before mapping
        var rawResponse = await client.ExecuteAsync(request);
        Console.WriteLine($"[RestSharp] Response Status: {rawResponse.StatusCode}");
        Console.WriteLine($"[RestSharp] Response Content: {rawResponse.Content}");
        
        // Deserialize the response to type T
        T response;
        try
        {
            response = JsonConvert.DeserializeObject<T>(rawResponse.Content ?? string.Empty);
            if (response == null)
            {
                Console.WriteLine($"[RestSharp] WARNING: Deserialization returned null for type {typeof(T).Name}");
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[RestSharp] ERROR: Failed to deserialize response to {typeof(T).Name}");
            Console.WriteLine($"[RestSharp] Error Details: {ex.Message}");
            throw new Exception($"Failed to deserialize response to {typeof(T).Name}: {ex.Message}", ex);
        }
        
        return (request, response);
        }
        public static IRestResponse Get(string baseURL, string url, Dictionary<string, string> headers = null)
        {
            var client = new RestClient(baseURL);

            var request = new RestRequest(url);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    // do something with entry.Value or entry.Key
                    request.AddHeader(entry.Key, entry.Value);
                }
            }
            var response =  client.Get(request);
            return response;
        }

       public static async Task<T> PostAsync<T>(string baseURL, string url, object body, Dictionary<string, string> headers = null)
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(url).AddJsonBody(body);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    // do something with entry.Value or entry.Key
                    request.AddHeader(entry.Key, entry.Value);
                }
            }
            var response = await client.PostAsync<T>(request);
            return response;
        }

        public static (IRestRequest request, IRestResponse response) Post(string baseURL, string url, object body = null, ICollection<KeyValuePair<string, string>> headers = null)
        {
            var client = new RestClient(baseURL);

            var request = new RestRequest(url, Method.POST).AddJsonBody(body);
            request.AddHeaders(headers);
            Request = request;
            Response = client.Post(request);
            return (request, Response);

        }

        public static async Task<T> PutAsync<T>(string baseURL, string url, object body, Dictionary<string, string> headers = null)
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(url).AddJsonBody(body);
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> entry in headers)
                {
                    // do something with entry.Value or entry.Key
                    request.AddHeader(entry.Key, entry.Value);
                }
            }
            var response = await client.PutAsync<T>(request);
            return response;
        }

    }
    public class QueryParam
    {
        public string Value { get; set; }
        public string Key { get; set; }
        public QueryParam() { }
        public QueryParam(string key ,string value) 
        {
            Value = value;
            Key = key;
        }

    }