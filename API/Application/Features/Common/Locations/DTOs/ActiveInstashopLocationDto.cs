namespace API.Application.Features.Common.Locations.DTOs
{
    public class ActiveInstashopLocationDto
    {
        public long LocationID { get; set; }
        public string LocationName { get; set; }
        public long CompanyID { get; set; }
        public DateTime? LastSuccessfulScrapeTime { get; set; }
    }
}
