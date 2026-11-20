using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Common;

public static class StatusRangeClipper
{
    public static bool HasDateRange(DateTime? startDate, DateTime? endDate) =>
        startDate.HasValue || endDate.HasValue;

    public static ClippedStatusPeriod Clip(
        Status status,
        DateTime? startDate,
        DateTime? endDateExclusive)
    {
        var originalEnd = status.UpdatedAt ?? status.CreatedAt.AddSeconds(Math.Max(0, status.Duration));
        var endBoundary = endDateExclusive?.AddSeconds(-1);

        var clippedStart = startDate.HasValue && status.CreatedAt < startDate.Value
            ? startDate.Value
            : status.CreatedAt;

        var clippedEnd = endBoundary.HasValue && originalEnd > endBoundary.Value
            ? endBoundary.Value
            : originalEnd;

        if (clippedEnd < clippedStart)
            clippedEnd = clippedStart;

        var duration = (int)Math.Max(
            0,
            Math.Floor((clippedEnd - clippedStart).TotalSeconds));

        var shouldReturnEnd = status.UpdatedAt.HasValue ||
                              (endBoundary.HasValue && originalEnd > endBoundary.Value);

        return new ClippedStatusPeriod(
            clippedStart,
            shouldReturnEnd ? clippedEnd : null,
            duration,
            clippedEnd);
    }
}

public readonly record struct ClippedStatusPeriod(
    DateTime Start,
    DateTime? End,
    int Duration,
    DateTime EffectiveEnd);
