using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.DTOs;
using API.Domain.Entities.Client;
using API.Domain.Enums;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using MediatR;

namespace API.Infrastructure.MessageBroker.Consumers
{
    public class InstashopReviewConsumer : IBaseConsumer<InstashopReviewMessage>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InstashopReviewConsumer> _logger;

        public InstashopReviewConsumer(
            IMediator mediator,
            ILogger<InstashopReviewConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(InstashopReviewMessage message)
        {
            try
            {
                _logger.LogInformation("Consuming Instashop review for LocationId: {LocationID}, ClientId: {InstashopClientId}",
                    message.LocationID, message.InstashopClientId);

                // Convert message to InstashopReview DTO
                var review = new InstashopReview
                {
                    CreatedAt = message.CreatedAt,
                    Comment = message.Comment,
                    Area = message.Area,
                    ProductAccuracy = message.ProductAccuracy,
                    DeliverySpeed = message.DeliverySpeed
                };

                // Save the review
                await _mediator.Send(new SaveInstashopReviewsCommand(
                    message.LocationID,
                    new List<InstashopReview> { review },
                    DateTime.UtcNow,
                    message.LocationName,
                    1, // MaxReviews - single review
                    message.CompanyID,
                    SourceType.Instashop
                ));

                _logger.LogInformation("Successfully consumed Instashop review for LocationId: {LocationID}",
                    message.LocationID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming Instashop review for LocationId: {LocationID}",
                    message.LocationID);
                throw;
            }
        }
    }
}
