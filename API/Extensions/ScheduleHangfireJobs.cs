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

                //Schedule Talabat recent reviews job to run daily at 7 PM
                recurringJobManager.AddOrUpdate<TalabatRecentReviewsScheduler>(
                    "TalabatRecentReviewsScheduler",
                    job => job.Execute(),
                    "11 * * * *"
                //"0 19 * * *" // Run at 7:00 PM every day (19:00)
                );

                //BackgroundJob.Enqueue<ProcessNonProcessedReviewsScheduler>(job => job.Execute());


                //Schedule publish location review summary job to run every hour
                    recurringJobManager.AddOrUpdate<PublishLocationReviewSummaryScheduler>(
                        "PublishLocationReviewSummaryScheduler",
                        job => job.Execute(),
                        "1 * * * *" // Run every hour at minute 0
                    );

                //Schedule check Talabat location scrape status job to run every hour
                recurringJobManager.AddOrUpdate<CheckTalabatLocationScrapeStatusScheduler>(
                    "CheckTalabatLocationScrapeStatusScheduler",
                    job => job.Execute(),
                    "30 * * * *" // Run every hour at minute 30
                );
            }
        }
    }
}
