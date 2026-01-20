
using API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Commands;
using API.Application.Services;
using API.Shared.Models;
using MediatR;

namespace API.EndPoints
{
    public class DemoEndPoint : EndpointDefinition
    {
        public override void RegisterEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("/demo/hello",
                async (IMediator _mediator, CancellationToken cancellationToken) =>
                {
                    await _mediator.Send(new LaunchInstashopRecentReviewScraperOrchestrator(20, 50));
                    
                })
            .WithTags("Demo")
             ;
        }
    }
}
