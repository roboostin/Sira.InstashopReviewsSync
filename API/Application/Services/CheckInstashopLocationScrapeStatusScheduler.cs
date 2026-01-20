using API.Application.Features.Common.Locations.Commands;
using API.Application.Features.Common.Locations.Queries;
using Hangfire;
using MediatR;

namespace API.Application.Services
{
    public class CheckInstashopLocationScrapeStatusScheduler
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CheckInstashopLocationScrapeStatusScheduler> _logger;

        public CheckInstashopLocationScrapeStatusScheduler(
            IMediator mediator,
            ILogger<CheckInstashopLocationScrapeStatusScheduler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        [JobDisplayName("Check Instashop Location Scrape Status Job")]
        public async Task Execute()
        {
            try
            {

                var companiesResult = await _mediator.Send(new GetCompaniesWithActiveInstashopLocationsQuery())
                    .ConfigureAwait(false);

                if (!companiesResult.IsSuccess || companiesResult.Data == null || companiesResult.Data.Count == 0)
                {
                    return;
                }

                var companyIDs = companiesResult.Data;

                var companyIndex = 0;
                const int delayIntervalMinutes = 5;

                foreach (var companyID in companyIDs)
                {
                    if (companyIndex == 0)
                    {
                        // First company - enqueue immediately
                        BackgroundJob.Enqueue<IMediator>(mediator =>
                            mediator.Send(new CheckCompanyInstashopLocationScrapeStatusCommand(companyID), default(CancellationToken)));

                    }
                    else
                    {
                        // Schedule with delay to distribute load
                        var delay = TimeSpan.FromMinutes(companyIndex * delayIntervalMinutes);
                        BackgroundJob.Schedule<IMediator>(
                            mediator => mediator.Send(new CheckCompanyInstashopLocationScrapeStatusCommand(companyID), default(CancellationToken)),
                            delay);
                    }

                    companyIndex++;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing check Instashop location scrape status job");
                throw;
            }
        }
    }
}
