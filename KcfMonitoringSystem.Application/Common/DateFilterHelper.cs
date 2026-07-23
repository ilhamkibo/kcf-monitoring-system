namespace KcfMonitoringSystem.Application.Common;

public static class DateFilterHelper
{
    public static (DateTime? Start, DateTime? End)
        Normalize(DateTime? startDate, DateTime? endDate)
    {
        if (!startDate.HasValue && !endDate.HasValue)
            return (null, null);

        if (startDate.HasValue && endDate.HasValue)
        {
            var min = startDate.Value <= endDate.Value
                ? startDate.Value
                : endDate.Value;

            var max = startDate.Value >= endDate.Value
                ? startDate.Value
                : endDate.Value;

            return (
                min.Date,
                max.Date.AddDays(1)
            );
        }

        var single = (startDate ?? endDate)!.Value.Date;

        return (
            single,
            single.AddDays(1)
        );
    }
}
