namespace API.Application.Interfaces;
public interface IJobSchedule
{
    Type JobType { get; }
    string CronExpression { get; }
}