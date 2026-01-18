using System.IdentityModel.Tokens.Jwt;
using System.Text;
using API.Shared.Helpers;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace API.Helpers;

public static class HttpRequestHelper
{
    public static HttpContext Current => new HttpContextAccessor().HttpContext;

    public static string DoIt()
    {
        string Protocol = Current.Request.Protocol;
        return Protocol;
    }
    public static bool IsHeaderContainsKey(string key)
    {
        return Current.Request?.Headers?.Any(header => header.Key.ToLower() == key.ToLower() && !string.IsNullOrEmpty(header.Value)) ?? false;
    }
    public static string GetHeaderValue(string key)
    {
        try
        {
            StringValues header;
         
            if (Current != null)
            {
                
                Current.Request.Headers.TryGetValue(key.ToLower(), out header);

                if(string.IsNullOrEmpty(header.ToString()) && key.ToLower() == "token")
                {
                    StringValues tokenstr;

                    Current.Request.Headers.TryGetValue("Authorization".ToLower(), out tokenstr);
                    var token = tokenstr.ToString();
                    if (string.IsNullOrEmpty(token))
                        return "";
                    if (token.Contains("Bearer"))
                        token = token.Replace("Bearer", "").Trim();
                    Current.Request.Headers.Append(key, token);
                    //return token;
                    
                   
                }
                Current.Request.Headers.TryGetValue(key.ToLower(), out header);
                var value= header.ToString();
                if(key.ToLower()=="token" && value.StartsWith(Constants.TOKEN_PREFIX))
                {
                    value = value.Remove(0, Constants.TOKEN_PREFIX.Length );
                    value= SecurityHelper.Decrypt(value);
                }
                    
                return value;
            }
            else 
                return "";

        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public static async System.Threading.Tasks.Task<string> CallGetRequestAsync(string url)
    {
        HttpClient client = new HttpClient();
        var responseString = client.GetStringAsync(url).ConfigureAwait(true).GetAwaiter().GetResult();
        return responseString;
    }
    public static long? GetJwtIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); // No validation

            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

            if (jtiClaim == null || !long.TryParse(jtiClaim.Value, out var jwtId))
            {
                return null;
            }

            return jwtId;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public static bool CallPostRequest(string url, object viewModel)
    {
        using (HttpClient client = new HttpClient())
        {
            string jsonInString = JsonConvert.SerializeObject(viewModel);

            client.PostAsync(url, new StringContent(jsonInString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
        }
        return true;

    }

    public static async Task<T> CallPostRequestAsync<T>(string url, object viewModel)
    {
        using (HttpClient client = new HttpClient())
        {
            string jsonInString = JsonConvert.SerializeObject(viewModel);

            var response = await client.PostAsync(url, new StringContent(jsonInString, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

    }

    public static async Task  CallPutRequestAsync(string url, object viewModel)
    {
        using (HttpClient client = new HttpClient())
        {
            string jsonInString = JsonConvert.SerializeObject(viewModel);

            var response = await client.PutAsync(url, new StringContent(jsonInString, Encoding.UTF8, "application/json"));
        }
    }

    public static async Task<T> CallPutRequestAsync<T>(string url, object viewModel)
    {
        using (HttpClient client = new HttpClient())
        {
            
            string jsonInString = JsonConvert.SerializeObject(viewModel);

            var response = await client.PutAsync(url, new StringContent(jsonInString, Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }

    }

    public static bool IsLocal(this HttpRequestMessage request)
    {
        try
        {
            var isLocal = request.Properties["MS_IsLocal"] as Lazy<bool>;
            return isLocal != null && isLocal.Value;
        }
        catch
        {
            return false;
        }

    }
    public static string GetBaseAddress()
    {
        try
        {
            HttpContext request = new HttpContextAccessor().HttpContext;
            var address = string.Format("{0}://{1}", request.Request.Scheme, request.Request.Host);
            if (address.ToLower().Contains("www.tayar.app"))
                return "https://tay-api.roboost.app";

            return address.Replace("http:", "https:");
        }
        catch (Exception ex)
        {
            return ConfigurationHelper.GetURL();
        }
    }


    public static string GetUrl()
    {
        HttpContext context = new HttpContextAccessor().HttpContext;
        string queryString = "";
        try
        {
            queryString = context.Request.QueryString.ToString();
        }
        catch
        {

        }

        return context.Request.Path + queryString;
    }
    public static string GetPath()
    {
        HttpContext context = new HttpContextAccessor().HttpContext;
        return context.Request.Path;
    }
    public static string GetIP()
    {
        try
        {
            HttpContext request = new HttpContextAccessor().HttpContext;
            return request.Connection.RemoteIpAddress.ToString();
        }
        catch (Exception ex)
        {
            return "";
        }
    }
    public static string GetUserAgent()
    {
        HttpContext request = new HttpContextAccessor().HttpContext;
        return request.Request.Headers["User-Agent"];
    }


    public static bool IsStaging()
    {
       return ConfigurationHelper.IsEnvironment("Staging");
        //HttpContext request = new HttpContextAccessor().HttpContext;
        //var address = string.Format("{0}://{1}", request.Request.Scheme, request.Request.Host);
        //return address.ToLower().Contains("staging");
    }

    public static bool IsProduction()
    {
        return ConfigurationHelper.IsEnvironment("Production");

        //HttpContext request = new HttpContextAccessor().HttpContext;
        //var address = string.Format("{0}://{1}", request.Request.Scheme, request.Request.Host).ToLower();
        //return address.ToLower().Contains("//api.tayar.app");
    }

    public static bool IsTesting()
    {
        return ConfigurationHelper.IsEnvironment("Testing");

        //HttpContext request = new HttpContextAccessor().HttpContext;
        //var address = string.Format("{0}://{1}", request.Request.Scheme, request.Request.Host).ToLower();
        //return address.ToLower().Contains("//api.tayar.info");
    }

    public static bool IsDevelopment()
    {
        return !IsProduction() && !IsTesting() && !IsStaging();
    }
    public static void AddDelayedJob(string url, int seconds)
    {
        string api = GetBaseAddress();
        Task.Delay(seconds * 1000).ContinueWith(t => CallGetRequestAsync($"{api}/{url}")).ConfigureAwait(false);
    }

    public static async Task<string> PutAsync(string api, object body, string mediaType = "text/html", ICollection<KeyValuePair<string, string>> headers = null)
    {
        HttpClient client = new HttpClient();
        using StringContent jsonContent = new(
            System.Text.Json.JsonSerializer.Serialize(body),
            Encoding.UTF8
            , mediaType);
        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);

        }
        //client.DefaultRequestHeaders.Add("api-key", "66AVZIW92GXN7UNH7J8X5MWALIEGNV6C");
        using HttpResponseMessage response = await client.PutAsync(
            api, jsonContent);
        var jsonResponse = await response.Content.ReadAsStringAsync();

        return jsonResponse;

    }
}