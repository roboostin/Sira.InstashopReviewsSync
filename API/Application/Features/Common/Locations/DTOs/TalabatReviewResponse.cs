using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace API.Application.Features.Common.Locations.DTOs
{
    public class TalabatReviewResponse
    {
        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonProperty("reviews")]
        [JsonPropertyName("reviews")]
        public ReviewsData Reviews { get; set; } = new ReviewsData();
    }

    public class ReviewsData
    {
        [JsonProperty("trt")]
        [JsonPropertyName("trt")]
        public string TotalReviews { get; set; } = string.Empty;

        [JsonProperty("rat")]
        [JsonPropertyName("rat")]
        public double Rating { get; set; }

        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonProperty("count")]
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonProperty("reviews")]
        [JsonPropertyName("reviews")]
        public List<Review> Reviews { get; set; } = new List<Review>();
    }

    public class Review
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int ID { get; set; }

        [JsonProperty("fn")]
        [JsonPropertyName("fn")]
        public string FirstName { get; set; } = string.Empty;

        [JsonProperty("oid")]
        [JsonPropertyName("oid")]
        public long OrderID { get; set; }

        [JsonProperty("rew")]
        [JsonPropertyName("rew")]
        public string Feedback { get; set; } = string.Empty;

        [JsonProperty("rat")]
        [JsonPropertyName("rat")]
        public int Rate { get; set; }

        [JsonProperty("con")]
        [JsonPropertyName("con")]
        public string Date { get; set; } = string.Empty;
    }
}
