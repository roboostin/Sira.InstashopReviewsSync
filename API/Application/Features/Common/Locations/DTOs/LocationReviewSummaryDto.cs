namespace API.Application.Features.Common.Locations.DTOs
{
    public class LocationReviewSummaryDto
    {
        public long LocationID { get; set; }
        public long CompanyID { get; set; }
        public double Rating { get; set; }
        public int TotalResponseCount { get; set; }
    }
}
