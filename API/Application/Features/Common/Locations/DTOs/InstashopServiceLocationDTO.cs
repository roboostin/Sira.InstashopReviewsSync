using System.Text.Json.Serialization;

namespace API.Application.Features.Common.Locations.DTOs
{
    public class InstashopServiceLocationDTO
    {
        [JsonPropertyName("locationID")]
        public long LocationID { get; set; }
        
        [JsonPropertyName("instashopClientId")]
        public string InstashopClientId { get; set; }
        
        [JsonPropertyName("locationName")]
        public string LocationName { get; set; }
        
        [JsonPropertyName("companyID")]
        public long CompanyID { get; set; }
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
