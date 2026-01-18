using System.Reflection;
using API.Application.Interfaces;
using API.Helpers.Attributes;
using API.Shared.Models;
using Quartz;
using Serilog;

namespace API.Helpers;
public static class JobRegistrationHelper
{
    public static IEnumerable<IJobSchedule> LoadJobsFromAssembly(Assembly assembly)
    {
        var jobSchedules = new List<IJobSchedule>();

        var jobTypes = assembly
            .GetTypes()
            .Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var jobType in jobTypes)
        {
            var cronAttr = jobType.GetCustomAttribute<CronScheduleAttribute>();
            var cronExpression = cronAttr?.CronExpression;

            if (!CronExpression.IsValidExpression(cronExpression))
            {
                Log.Warning($"Skipping job {jobType.Name}: Invalid cron expression '{cronExpression}'");
                continue;
            }

            jobSchedules.Add(new JobSchedule(jobType, cronExpression));
        }

        return jobSchedules;
    }
}