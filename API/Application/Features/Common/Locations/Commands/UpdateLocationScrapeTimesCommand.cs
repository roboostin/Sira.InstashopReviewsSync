using API.Domain.Entities;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpdateLocationScrapeTimesCommand(
        long LocationID,
        DateTime? LastScrapeAttemptTime = null,
        DateTime? LastSuccessfulScrapeTime = null
    ) : IRequest<RequestResult<bool>>;

    public class UpdateLocationScrapeTimesCommandHandler : IRequestHandler<UpdateLocationScrapeTimesCommand, RequestResult<bool>>
    {
        private readonly IRepository<Location> _repository;

        public UpdateLocationScrapeTimesCommandHandler(
            IRepository<Location> repository)
        {
            _repository = repository;
        }

        public async Task<RequestResult<bool>> Handle(UpdateLocationScrapeTimesCommand request, CancellationToken cancellationToken)
        {

            _repository.SaveIncluded(new Location
            {
                ID = request.LocationID,
                LastScrapeAttemptTime = request.LastScrapeAttemptTime,
                LastSuccessfulScrapeTime = request.LastSuccessfulScrapeTime
            }
            , request.LastSuccessfulScrapeTime != null ? nameof(Location.LastSuccessfulScrapeTime) : null
            , request.LastScrapeAttemptTime != null ? nameof(Location.LastScrapeAttemptTime) : null);

            return RequestResult<bool>.Success(true);
        }
    }
}

