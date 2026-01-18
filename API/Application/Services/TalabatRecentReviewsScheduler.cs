using API.Application.Features.Locations.LaunchTalabatRecentReviewScraper.Commands;
using API.Domain.UnitOfWork;
using Hangfire;
using MediatR;

namespace API.Application.Services
{
    public class TalabatRecentReviewsScheduler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TalabatRecentReviewsScheduler> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TalabatRecentReviewsScheduler(
            IMediator mediator,
            ILogger<TalabatRecentReviewsScheduler> logger,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [AutomaticRetry(Attempts = 3)]
        [JobDisplayName("Talabat Recent Reviews Import Job")]
        public async Task Execute()
        {
            try
            {
                _logger.LogInformation("Starting Talabat reviews import job for the last 24 hours");

                const int limitPerRequest = 5;
                const int maxReviews = 100;

                var result = await _mediator.Send(new LaunchTalabatRecentReviewScraperOrchestrator(
                    limitPerRequest,
                    maxReviews)).ConfigureAwait(false);

                _logger.LogInformation("Talabat reviews import for the last 24 hours completed: {Result}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing Talabat recent reviews import job");
            }
        }
    }
}
