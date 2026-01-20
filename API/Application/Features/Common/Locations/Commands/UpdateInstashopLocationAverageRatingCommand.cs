using API.Domain.Entities;
using API.Domain.Enums;
using API.Infrastructure.Persistence.Repositories;
using API.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpdateInstashopLocationAverageRatingCommand(
        long LocationID
    ) : IRequest<RequestResult<Unit>>;

    public class UpdateInstashopLocationAverageRatingCommandHandler : IRequestHandler<UpdateInstashopLocationAverageRatingCommand, RequestResult<Unit>>
    {
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<Domain.Entities.Client.Review> _reviewRepository;
        private readonly ILogger<UpdateInstashopLocationAverageRatingCommandHandler> _logger;

        public UpdateInstashopLocationAverageRatingCommandHandler(
            IRepository<Location> locationRepository,
            IRepository<Domain.Entities.Client.Review> reviewRepository,
            ILogger<UpdateInstashopLocationAverageRatingCommandHandler> logger)
        {
            _locationRepository = locationRepository;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<RequestResult<Unit>> Handle(UpdateInstashopLocationAverageRatingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Calculate average rating from all Instashop reviews for this location
                var averageRating = await _reviewRepository
                    .Get(r => r.LocationID == request.LocationID && r.Source == SourceType.Instashop)
                    .Select(r => (double?)r.Rate)
                    .DefaultIfEmpty()
                    .AverageAsync(cancellationToken);

                if (averageRating.HasValue)
                {
                    _locationRepository.SaveIncluded(
                        new Location
                        {
                            ID = request.LocationID,
                            InstashopAverageRating = averageRating.Value
                        },
                        nameof(Location.InstashopAverageRating)
                    );

                    await _locationRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Updated InstashopAverageRating for LocationID {LocationID} to {Rating}",
                        request.LocationID, averageRating.Value);
                }
                else
                {
                    _logger.LogInformation("No Instashop reviews found for LocationID {LocationID}, skipping average rating update",
                        request.LocationID);
                }

                return RequestResult<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating InstashopAverageRating for LocationID {LocationID}", request.LocationID);
                return RequestResult<Unit>.Failure(Domain.Enums.ErrorCode.None, $"Error updating average rating: {ex.Message}");
            }
        }
    }
}
