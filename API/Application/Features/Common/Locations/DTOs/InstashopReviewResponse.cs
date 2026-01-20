using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace API.Application.Features.Common.Locations.DTOs
{
    public class InstashopReviewResponse
    {
        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonProperty("count")]
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonProperty("reviews")]
        [JsonPropertyName("reviews")]
        public List<InstashopReview> Reviews { get; set; } = new List<InstashopReview>();
    }

    public class InstashopReview
    {
        [JsonProperty("CreatedAt")]
        [JsonPropertyName("CreatedAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonProperty("Comment")]
        [JsonPropertyName("Comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonProperty("Area")]
        [JsonPropertyName("Area")]
        public string Area { get; set; } = string.Empty;

        [JsonProperty("productAccuracy")]
        [JsonPropertyName("productAccuracy")]
        public int ProductAccuracy { get; set; }

        [JsonProperty("deliverySpeed")]
        [JsonPropertyName("deliverySpeed")]
        public int DeliverySpeed { get; set; }
    }
}
