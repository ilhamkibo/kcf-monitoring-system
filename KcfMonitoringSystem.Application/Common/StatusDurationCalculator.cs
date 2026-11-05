using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Common;

public static class StatusDurationCalculator
{
    public static double CalculateSeconds(Status status)
    {
        return status.UpdatedAt.HasValue
            ? (status.UpdatedAt.Value - status.CreatedAt).TotalSeconds
            : 0;
    }

    public static int CalculateWholeSeconds(Status status)
    {
        return (int)CalculateSeconds(status);
    }
}
