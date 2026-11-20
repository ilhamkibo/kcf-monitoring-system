using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Tests;

public class StatusRangeClipperTests
{
    [Fact]
    public void Clip_TrimsStartAndRecalculatesDuration()
    {
        var status = new Status
        {
            CreatedAt = new DateTime(2026, 10, 31, 22, 0, 0),
            UpdatedAt = new DateTime(2026, 11, 2, 6, 0, 0),
            Duration = 115_200
        };

        var result = StatusRangeClipper.Clip(
            status,
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 6));

        Assert.Equal(new DateTime(2026, 11, 1), result.Start);
        Assert.Equal(new DateTime(2026, 11, 2, 6, 0, 0), result.End);
        Assert.Equal(108_000, result.Duration);
    }

    [Fact]
    public void Clip_TrimsEndToLastSecondAndRecalculatesDuration()
    {
        var status = new Status
        {
            CreatedAt = new DateTime(2026, 11, 5, 21, 0, 0),
            UpdatedAt = new DateTime(2026, 11, 6, 6, 0, 0),
            Duration = 32_400
        };

        var result = StatusRangeClipper.Clip(
            status,
            new DateTime(2026, 11, 1),
            new DateTime(2026, 11, 6));

        Assert.Equal(new DateTime(2026, 11, 5, 21, 0, 0), result.Start);
        Assert.Equal(new DateTime(2026, 11, 5, 23, 59, 59), result.End);
        Assert.Equal(10_799, result.Duration);
    }
}
