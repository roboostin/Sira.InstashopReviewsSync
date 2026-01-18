using System.Text.Json.Serialization;

namespace API.Application.Features.Common.Locations.DTOs
{
    public class TalabatServiceLocationDTO
    {
        [JsonPropertyName("locationID")]
        public long LocationID { get; set; }
        
        [JsonPropertyName("talabatLocationsIDs")]
        public List<int> TalabatLocationsIDs { get; set; }
        
        [JsonPropertyName("locationName")]
        public string LocationName { get; set; }
        
        [JsonPropertyName("companyID")]
        public long CompanyID { get; set; }
        
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}

