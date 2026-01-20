using API.Application.Features.Locations.LaunchInstashopReviewScraper.Commands;
using API.Shared.Models;
using MediatR;

namespace API.EndPoints.Locations.LaunchInstashopReviewScraper
{
    public class LaunchInstashopReviewScraperEndPoint : EndpointDefinition
    {
        public override void RegisterEndpoints(IEndpointRouteBuilder app)
        {
            app.MapPost("/Location/LaunchInstashopReviewScraper",
                async (LaunchInstashopReviewScraperRequest request, IMediator mediator, CancellationToken cancellationToken) =>
                {
                    if (request == null)
                    {
                        return Response(RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None, "Request is null"));
                    }

                    var result = await mediator.Send(new LaunchInstashopReviewScraperOrchestrator(
                        request.LocationId,
                        request.InstashopClientId,
                        request.TimeStamp,
                        request.LocationName,
                        request.CompanyID,
                        request.LimitPerRequest ?? 20,
                        request.MaxReviews ?? 50
                    ), cancellationToken);

                    return Response(result);
                })
            .WithTags("Location")
            .WithName("LaunchInstashopReviewScraper")
            .Accepts<LaunchInstashopReviewScraperRequest>("application/json")
            .Produces<EndPointResponse<bool>>();
        }
    }

    public class LaunchInstashopReviewScraperRequest
    {
        public long LocationId { get; set; }
        public string InstashopClientId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string LocationName { get; set; }
        public long CompanyID { get; set; }
        public int? LimitPerRequest { get; set; }
        public int? MaxReviews { get; set; }
    }
}
