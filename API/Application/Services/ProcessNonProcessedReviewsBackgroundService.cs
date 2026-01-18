using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.Queries;
using API.Domain.UnitOfWork;
using API.Infrastructure.MessageBroker;
using API.Infrastructure.MessageBroker.Core;
using API.Infrastructure.MessageBroker.Messages;
using API.Shared.Models;
using MediatR;

namespace API.Application.Services;

public class ProcessNonProcessedReviewsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProcessNonProcessedReviewsBackgroundService> _logger;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // Prevent concurrent execution

    public ProcessNonProcessedReviewsBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProcessNonProcessedReviewsBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProcessNonProcessedReviewsBackgroundService started");

        TimeSpan nextDelay = TimeSpan.FromSeconds(30);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Use semaphore to prevent concurrent execution
                if (await _semaphore.WaitAsync(0, stoppingToken))
                {
                    try
                    {
                        nextDelay = await ProcessReviewsAsync(stoppingToken);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                else
                {
                    nextDelay = TimeSpan.FromSeconds(5);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                nextDelay = TimeSpan.FromSeconds(5);
            }

            await Task.Delay(nextDelay, stoppingToken);
        }

        _logger.LogInformation("ProcessNonProcessedReviewsBackgroundService stopped");
    }

    private async Task<TimeSpan> ProcessReviewsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        int reviewsLimit = 51;

        try
        {
            var queryResult = await mediator.Send(new GetNonProcessedReviewsQuery(Limit: reviewsLimit), cancellationToken);

            var reviews = queryResult.Data;

            if (reviews == null || !reviews.Any())
            {
                return TimeSpan.FromMinutes(10);
            }

            int reviewCount = reviews.Count;
            int batchSize = reviewCount == reviewsLimit ? reviewsLimit - 1 : reviewCount;
            reviews = reviews.Take(batchSize).ToList();


            var processedReviewIds = reviews.Select(r => r.ID).ToList();

            foreach (var review in reviews)
            {
                var routingKey = API.Infrastructure.MessageBroker.Constants.MessageBrokerAggregatorRoutingKey;
                var message = new ReviewMessage
                {
                    ID = review.ID,
                    LocationID = review.LocationID ?? 0,
                    LocationName = review.LocationName ?? string.Empty,
                    Feedback = review.Feedback ?? string.Empty,
                    ReviewerName = review.ReviewerName ?? string.Empty,
                    PublishedAt = review.PublishedAt ?? DateTime.UtcNow,
                    ReviewDate = review.ReviewDate,
                    Sentiment = review.Sentiment ?? string.Empty,
                    Rate = review.Rate,
                    CompanyID = review.CompanyID,
                    PublisherRoutingKey = routingKey,
                    ACKRoutingKey = Constants.MessageBrokerReviewACKRoutingKey
                };

                var eventMessage = new MessageBrokerEvent(message, routingKey);
                eventPublisher.AddEvent(eventMessage);
            }

            eventPublisher.Publish();

            if (processedReviewIds.Any())
            {
                var markResult = await mediator.Send(new MarkReviewsAsProcessedCommand(processedReviewIds), cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            if (reviewCount == 101)
            {
                return TimeSpan.FromSeconds(30); 
            }
            else
            {
                return TimeSpan.FromMinutes(10);
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _semaphore?.Dispose();
        base.Dispose();
    }
}

