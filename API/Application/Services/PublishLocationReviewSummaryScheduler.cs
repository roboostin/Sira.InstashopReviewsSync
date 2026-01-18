using API.Application.Features.Common.Locations.Queries;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using API.Shared.Models;
using Hangfire;
using MediatR;

namespace API.Application.Services
{
    public class PublishLocationReviewSummaryScheduler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PublishLocationReviewSummaryScheduler> _logger;
        private readonly IEventPublisher _eventPublisher;

        public PublishLocationReviewSummaryScheduler(
            IMediator mediator,
            ILogger<PublishLocationReviewSummaryScheduler> logger,
            IEventPublisher eventPublisher)
        {
            _mediator = mediator;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        [AutomaticRetry(Attempts = 3)]
        [JobDisplayName("Publish Location Review Summary Job")]
        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("Starting publish location review summary job");

                // Get all locations with their review summary data
                var locationsWithSummary = await _mediator.Send(new GetLocationsWithReviewSummaryQuery());

                if (locationsWithSummary.IsSuccess == false || locationsWithSummary.Data.Count == 0)
                {
                    _logger.LogInformation("No locations with review summary found");
                    return;
                }

                int successCount = 0;
                int errorCount = 0;

                // Publish review summary message for each location
                foreach (var locationSummary in locationsWithSummary.Data)
                {
                    try
                    {
                        // Skip locations with no reviews
                        if (locationSummary.TotalResponseCount == 0)
                        {
                            _logger.LogDebug("Skipping location {LocationID} - no reviews", locationSummary.LocationID);
                            continue;
                        }

                        var message = new ReviewSummaryMessage
                        {
                            LocationID = locationSummary.LocationID,
                            CompanyID = locationSummary.CompanyID,
                            Rating = locationSummary.Rating,
                            TotalResponseCount = locationSummary.TotalResponseCount,
                            MessageCreatedDate = DateTime.UtcNow
                        };

                        var routingKey = API.Infrastructure.MessageBroker.Constants.MessageBrokerAggregatorRoutingKey;
                        var eventMessage = new MessageBrokerEvent(message, routingKey);
                        _eventPublisher.AddEvent(eventMessage);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        _logger.LogError(ex, "Error creating event for location {LocationID}", locationSummary.LocationID);
                    }
                }

                // Publish all events
                if (successCount > 0)
                {
                    _eventPublisher.Publish();
                    _logger.LogInformation("Successfully published review summary for {SuccessCount} locations. Errors: {ErrorCount}", 
                        successCount, errorCount);
                }
                else
                {
                    _logger.LogInformation("No review summaries to publish. Errors: {ErrorCount}", errorCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing publish location review summary job");
                throw;
            }
        }
    }
}

