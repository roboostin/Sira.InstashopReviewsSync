using API.Application.Features.Locations.LaunchInstashopRecentReviewScraper.Commands;
using API.Domain.UnitOfWork;
using Hangfire;
using MediatR;

namespace API.Application.Services
{
    public class InstashopRecentReviewsScheduler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InstashopRecentReviewsScheduler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public InstashopRecentReviewsScheduler(
            IMediator mediator,
            ILogger<InstashopRecentReviewsScheduler> logger,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [AutomaticRetry(Attempts = 3)]
        [JobDisplayName("Instashop Recent Reviews Import Job")]
        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("Starting Instashop reviews import job for the last 24 hours");

                const int limitPerRequest = 20;
                const int maxReviews = 50;

                var result = await _mediator.Send(new LaunchInstashopRecentReviewScraperOrchestrator(
                    limitPerRequest,
                    maxReviews)).ConfigureAwait(false);

                _logger.LogInformation("Instashop reviews import for the last 24 hours completed: {Result}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing Instashop recent reviews import job");
            }
        }
    }
}
