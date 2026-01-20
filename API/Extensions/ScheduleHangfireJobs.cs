using API.Application.Services;
using API.Config.Hangfire;
using Hangfire;

namespace API.Extensions
{
    public static class ScheduleHangfireJobsExtensions
    {

        public static void ScheduleHangfireJobs(this IApplicationBuilder app, IConfiguration configuration)
        {
            var hangfireSettings = configuration.GetSection("HangfireSettings").Get<HangfireSettings>();

            if (hangfireSettings?.Enabled == true)
            {
                var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

                //Schedule Instashop recent reviews job to run daily at 8 PM
                recurringJobManager.AddOrUpdate<InstashopRecentReviewsScheduler>(
                    "InstashopRecentReviewsScheduler",
                    job => job.Execute(),
                    "0 20 * * *" // Run at 8:00 PM every day (20:00)
                );

                //BackgroundJob.Enqueue<ProcessNonProcessedReviewsScheduler>(job => job.Execute());


                //Schedule publish location review summary job to run every hour
                    recurringJobManager.AddOrUpdate<PublishLocationReviewSummaryScheduler>(
                        "PublishLocationReviewSummaryScheduler",
                        job => job.Execute(),
                        "1 * * * *" // Run every hour at minute 0
                    );

                //Schedule check Instashop location scrape status job to run every hour
                recurringJobManager.AddOrUpdate<CheckInstashopLocationScrapeStatusScheduler>(
                    "CheckInstashopLocationScrapeStatusScheduler",
                    job => job.Execute(),
                    "30 * * * *" // Run every hour at minute 30
                );
            }
        }
    }
}
