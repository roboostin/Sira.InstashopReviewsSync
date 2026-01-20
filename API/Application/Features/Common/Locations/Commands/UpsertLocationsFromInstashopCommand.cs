using API.Application.Features.Common.Locations.DTOs;
using API.Application.Features.Common.Locations.Queries;
using API.Shared.Models;
using MediatR;

namespace API.Application.Features.Common.Locations.Commands
{
    public record UpsertLocationsFromInstashopCommand(
        List<InstashopServiceLocationDTO> Locations) : IRequest<RequestResult<bool>>;

    public class UpsertLocationsFromInstashopCommandHandler : IRequestHandler<UpsertLocationsFromInstashopCommand, RequestResult<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpsertLocationsFromInstashopCommandHandler> _logger;

        public UpsertLocationsFromInstashopCommandHandler(
            IMediator mediator,
            ILogger<UpsertLocationsFromInstashopCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RequestResult<bool>> Handle(UpsertLocationsFromInstashopCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Locations == null || request.Locations.Count == 0)
                {
                    return RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None, "No locations provided");
                }

                // Build list of location keys for batch query
                var locationKeys = request.Locations
                    .Select(l => new LocationKey(l.LocationID, l.CompanyID))
                    .ToList();

                // Single batch query to get all existing locations
                var locationsQuery = new GetLocationsByLocationIdsAndCompanyIdsQuery(locationKeys);
                var existingLocationsResult = await _mediator.Send(locationsQuery, cancellationToken).ConfigureAwait(false);

                if (!existingLocationsResult.IsSuccess)
                {
                    return RequestResult<bool>.Failure(existingLocationsResult.ErrorCode, existingLocationsResult.Message);
                }

                var existingLocations = existingLocationsResult.Data ?? new List<LocationExistenceDto>();
                
                // Create a dictionary for quick lookup: (LocationID, CompanyID) -> LocationExistenceDto
                var existingLocationsDict = existingLocations
                    .ToDictionary(l => (l.LocationID, l.CompanyID), l => l);

                // Separate locations into those to add and those to update
                var locationsToAdd = new List<InstashopServiceLocationDTO>();
                var locationsToUpdate = new List<InstashopServiceLocationDTO>();

                foreach (var locationDto in request.Locations)
                {
                    var key = (locationDto.LocationID, locationDto.CompanyID);
                    if (existingLocationsDict.ContainsKey(key))
                    {
                        locationsToUpdate.Add(locationDto);
                    }
                    else
                    {
                        locationsToAdd.Add(locationDto);
                    }
                }

                // Process all updates
                foreach (var locationDto in locationsToUpdate)
                {
                    var updateCommand = new UpdateLocationFromInstashopCommand(
                        locationDto.LocationID,
                        locationDto);
                    
                    var updateResult = await _mediator.Send(updateCommand, cancellationToken).ConfigureAwait(false);
                }

                // Process all adds
                foreach (var locationDto in locationsToAdd)
                {
                    var addCommand = new AddLocationFromInstashopCommand(locationDto);
                    
                    var addResult = await _mediator.Send(addCommand, cancellationToken).ConfigureAwait(false);

                }


                return RequestResult<bool>.Success(true, $"Successfully processed {request.Locations.Count} locations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting locations from Instashop");
                return RequestResult<bool>.Failure(Domain.Enums.ErrorCode.None, $"An error occurred while processing locations: {ex.Message}");
            }
        }
    }
}
