namespace API.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CronScheduleAttribute : Attribute
{
    public string CronExpression { get; }
    public CronScheduleAttribute(string cronExpression)
    {
        CronExpression = cronExpression;
    }
}