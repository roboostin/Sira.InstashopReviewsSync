using API.Application.Features.Locations.LaunchInstashopReviewScraper.Commands;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.AddLocation
{
    public record AddLocationOrchestrator(
        long ID,
        string Name,
        string InstashopClientId,
        long CompanyID,
        DateTime CreatedAt,
        long CreatedBy,
        DateTime? UpdatedAt = null,
        long? UpdatedBy = null) : IRequest<RequestResult<Unit>>;

    public class AddLocationOrchestratorHandler : IRequestHandler<AddLocationOrchestrator, RequestResult<Unit>>
    {
        private readonly IMediator mediator;

        public AddLocationOrchestratorHandler(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<RequestResult<Unit>> Handle(AddLocationOrchestrator request, CancellationToken cancellationToken)
        {
            await mediator.Send(new AddLocationCommand(request.ID, request.Name, request.InstashopClientId
                , request.CompanyID, request.CreatedAt, request.CreatedBy));

            if (!string.IsNullOrEmpty(request.InstashopClientId))
            {
                await mediator.Send(new LaunchInstashopReviewScraperOrchestrator(
                    request.ID,
                    request.InstashopClientId,
                    DateTime.UtcNow.AddMonths(-3),
                    request.Name,
                    request.CompanyID,
                    20, // LimitPerRequest
                    50  // MaxReviews
                ));
            }

            return RequestResult<Unit>.Success();
        }
    }
}
