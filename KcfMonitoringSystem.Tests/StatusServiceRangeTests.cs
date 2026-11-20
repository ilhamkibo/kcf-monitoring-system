using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Domain.Entities;
using Moq;

namespace KcfMonitoringSystem.Tests;

public class StatusServiceRangeTests
{
    [Fact]
    public async Task GetTimelineAsync_ClipsStatusButPreservesProductionDates()
    {
        var productionStart = new DateTime(2026, 10, 31, 4, 12, 7);
        var productionEnd = new DateTime(2026, 11, 2, 9, 57, 53);
        var status = CreateStatus(
            new DateTime(2026, 10, 31, 22, 0, 0),
            new DateTime(2026, 11, 2, 6, 0, 0),
            productionStart,
            productionEnd);
        var repository = new Mock<IStatusRepository>();
        repository
            .Setup(x => x.GetTimelineStatusesAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync([status]);
        var service = new StatusService(repository.Object);

        var response = await service.GetTimelineAsync(new StatusFilter
        {
            StartDate = new DateTime(2026, 11, 1),
            EndDate = new DateTime(2026, 11, 5)
        });

        var production = Assert.Single(Assert.Single(response.Data).Production);
        var timeline = Assert.Single(production.Timeline);
        Assert.Equal(productionStart, production.Start);
        Assert.Equal(productionEnd, production.End);
        Assert.Equal(new DateTime(2026, 11, 1), timeline.Start);
        Assert.Equal(new DateTime(2026, 11, 2, 6, 0, 0), timeline.End);
        Assert.Equal(108_000, timeline.Duration);
    }

    [Fact]
    public async Task GetActivityAsync_SplitsClippedDurationAcrossDays()
    {
        var status = CreateStatus(
            new DateTime(2026, 11, 1, 23, 0, 0),
            new DateTime(2026, 11, 2, 1, 0, 0),
            new DateTime(2026, 11, 1, 20, 0, 0),
            new DateTime(2026, 11, 2, 2, 0, 0));
        var repository = new Mock<IStatusRepository>();
        repository
            .Setup(x => x.GetActivityStatusesAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync([status]);
        var service = new StatusService(repository.Object);

        var response = await service.GetActivityAsync(new StatusFilter
        {
            StartDate = new DateTime(2026, 11, 1),
            EndDate = new DateTime(2026, 11, 2)
        });

        Assert.Equal(2, response.Data.Count);
        Assert.All(response.Data, activity =>
            Assert.Equal(3_600, Assert.Single(activity.Details).TotalTime));
    }

    [Theory]
    [InlineData(MaintenanceStatusCodes.UnderMaintenance)]
    [InlineData(MaintenanceStatusCodes.PreventiveMaintenance)]
    public async Task GetTimelineAsync_UsesLatestMaintenanceReasonAsMessage(int statusCode)
    {
        var status = CreateStatus(
            new DateTime(2026, 11, 1, 10, 0, 0),
            new DateTime(2026, 11, 1, 11, 0, 0),
            new DateTime(2026, 11, 1, 10, 0, 0),
            new DateTime(2026, 11, 1, 11, 0, 0));
        status.Code = statusCode;
        status.Maintenances =
        [
            new Maintenance { Id = 1, Status = status, StatusId = status.Id, Reason = "Old reason" },
            new Maintenance { Id = 2, Status = status, StatusId = status.Id, Reason = "Latest reason" }
        ];

        var repository = new Mock<IStatusRepository>();
        repository
            .Setup(x => x.GetTimelineStatusesAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync([status]);
        var service = new StatusService(repository.Object);

        var response = await service.GetTimelineAsync(new StatusFilter());

        var timeline = Assert.Single(Assert.Single(Assert.Single(response.Data).Production).Timeline);
        Assert.Equal("Latest reason", timeline.Message);
    }

    private static Status CreateStatus(
        DateTime statusStart,
        DateTime statusEnd,
        DateTime productionStart,
        DateTime productionEnd)
    {
        var user = new User { Id = 1, Name = "Operator" };
        var product = new Product
        {
            Id = 1,
            ProductNo = "P001",
            PartName = "Part",
            PartNo = "PN001"
        };
        var production = new Production
        {
            Id = 1,
            User = user,
            UserId = user.Id,
            Product = product,
            ProductId = product.Id,
            CreatedAt = productionStart,
            UpdatedAt = productionEnd
        };

        return new Status
        {
            Id = 1,
            MachineId = 1,
            Machine = new Machine { Id = 1, Name = "Machine" },
            Production = production,
            ProductionId = production.Id,
            Code = 1,
            CreatedAt = statusStart,
            UpdatedAt = statusEnd,
            Duration = (int)(statusEnd - statusStart).TotalSeconds
        };
    }
}
