using API.Application.Interfaces;

namespace API.Shared.Models;

public class JobSchedule : IJobSchedule
{
    public Type JobType { get; }
    public string CronExpression { get; }

    public JobSchedule(Type jobType, string cronExpression)
    {
        JobType = jobType;
        CronExpression = cronExpression;
    }
}