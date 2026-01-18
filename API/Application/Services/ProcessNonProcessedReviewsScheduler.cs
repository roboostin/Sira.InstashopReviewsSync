//using API.Application.Features.Common.Locations.Commands;
//using API.Application.Features.Common.Locations.Queries;
//using API.Domain.UnitOfWork;
//using API.Infrastructure.MessageBroker;
//using API.Infrastructure.MessageBroker.Core;
//using API.Infrastructure.MessageBroker.Messages;
//using API.Shared.Models;
//using Hangfire;
//using MediatR;
//using Microsoft.Extensions.DependencyInjection;

//namespace API.Application.Services
//{
//    public class ProcessNonProcessedReviewsScheduler
//    {
//        private readonly IMediator _mediator;
//        private readonly ILogger<ProcessNonProcessedReviewsScheduler> _logger;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IServiceScopeFactory _serviceScopeFactory;

//        public ProcessNonProcessedReviewsScheduler(
//            IMediator mediator,
//            ILogger<ProcessNonProcessedReviewsScheduler> logger,
//            IUnitOfWork unitOfWork,
//            IServiceScopeFactory serviceScopeFactory)
//        {
//            _mediator = mediator;
//            _logger = logger;
//            _unitOfWork = unitOfWork;
//            _serviceScopeFactory = serviceScopeFactory;
//        }

//        [AutomaticRetry(Attempts = 3)]
//        [JobDisplayName("Process Non-Processed Reviews Job")]
//        public async Task Execute()
//        {
//            // Create a new scope only for EventPublisher to ensure a fresh instance each time
//            // This prevents scope leaks and ensures EventPublisher gets a new instance with empty events list
//            using var scope = _serviceScopeFactory.CreateScope();
//            var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

//            int reviewsLimit = 101;
//            try
//            {
//                _logger.LogInformation("Starting process non-processed reviews job");
//                return;
//                // Query for non-processed reviews
//                var queryResult = await _mediator.Send(new GetNonProcessedReviewsQuery(Limit: reviewsLimit));

//                if (!queryResult.IsSuccess)
//                {
//                    _logger.LogError("Failed to get non-processed reviews: {Message}", queryResult.Message);
//                    return;
//                }

//                var reviews = queryResult.Data;

//                if (reviews == null || !reviews.Any())
//                {
//                    _logger.LogInformation("No non-processed reviews found");
//                    BackgroundJob.Schedule<ProcessNonProcessedReviewsScheduler>(
//                        job => job.Execute(),
//                        TimeSpan.FromSeconds(10*60)
//                    );
//                    return;
//                }

//                int reviewCount = reviews.Count;

//                int batchSize = reviewCount == reviewsLimit ? reviewsLimit - 1 : reviewCount;

//                var reviewsToProcess = reviews.Take(batchSize).ToList();

//                _logger.LogInformation("Found {Count} non-processed reviews. Processing batch of {BatchSize}", reviewCount, batchSize);

//                var processedReviewIds = new List<long>();


//                foreach (var review in reviewsToProcess)
//                {
//                    try
//                    {
//                        var routingKey = API.Infrastructure.MessageBroker.Constants.MessageBrokerAggregatorRoutingKey;
//                        var message = new ReviewMessage
//                        {
//                            ID = review.ID,
//                            LocationID = review.LocationID ?? 0,
//                            LocationName = review.LocationName ?? string.Empty,
//                            Feedback = review.Feedback ?? string.Empty,
//                            ReviewerName = review.ReviewerName ?? string.Empty,
//                            PublishedAt = review.PublishedAt ?? DateTime.UtcNow,
//                            ReviewDate = review.ReviewDate,
//                            Sentiment = review.Sentiment ?? string.Empty,
//                            Rate = review.Rate,
//                            CompanyID = review.CompanyID,
//                            PublisherRoutingKey = routingKey,
//                            ACKRoutingKey = Constants.MessageBrokerReviewACKRoutingKey
//                        };

//                        var eventMessage = new MessageBrokerEvent(message, routingKey);
//                        eventPublisher.AddEvent(eventMessage);
//                        processedReviewIds.Add(review.ID);
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Error creating event for review {ReviewId}", review.ID);
//                    }
//                }

//                eventPublisher.Publish();

//                if (processedReviewIds.Any())
//                {
//                    try
//                    {
//                        var markResult = await _mediator.Send(new MarkReviewsAsProcessedCommand(processedReviewIds));

//                        if (!markResult.IsSuccess)
//                        {
//                            _logger.LogError("Failed to mark reviews as processed: {Message}", markResult.Message);
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        _logger.LogError(ex, "Failed to mark reviews as processed");
//                    }
//                }

//                await _unitOfWork.SaveChangesAsync();

//                if (reviewCount == 101)
//                {
//                    BackgroundJob.Schedule<ProcessNonProcessedReviewsScheduler>(
//                        job => job.Execute(),
//                        TimeSpan.FromSeconds(10*30)
//                    );
//                    _logger.LogInformation("More reviews available. Scheduled next batch in 30 seconds");
//                }
//                else
//                {
//                    BackgroundJob.Schedule<ProcessNonProcessedReviewsScheduler>(
//                        job => job.Execute(),
//                        TimeSpan.FromSeconds(10*60)
//                    );
//                    _logger.LogInformation("All available reviews processed. Recurring job will check again in 1 minute");
//                }

//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while executing process non-processed reviews job");
//                throw;
//            }
//        }
//    }
//}

