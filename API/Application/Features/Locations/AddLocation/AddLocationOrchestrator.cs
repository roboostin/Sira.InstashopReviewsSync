using API.Application.Features.Locations.LaunchTalabatReviewScraper.Commands;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Locations.AddLocation
{
    public record AddLocationOrchestrator(
        long ID,
        string Name,
        List<int> TalabatLocationIDs,
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
            await mediator.Send(new AddLocationCommand(request.ID, request.Name, request.TalabatLocationIDs
                , request.CompanyID, request.CreatedAt, request.CreatedBy));

            await mediator.Send(new LaunchTalabatReviewScraperOrchestrator(
                request.ID,
                request.TalabatLocationIDs,
                DateTime.Now.AddMonths(-3),
                request.Name,
                request.CompanyID
            ));

            return RequestResult<Unit>.Success();
        }
    }
}
