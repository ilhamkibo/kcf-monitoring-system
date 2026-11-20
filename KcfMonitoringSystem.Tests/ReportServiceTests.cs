using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Domain.Entities;
using Moq;

namespace KcfMonitoringSystem.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task GetProductionRecordsAsync_UsesFirstRunningStatusAsClassificationBoundary()
    {
        var production = CreateProductionWithStatuses();
        var repository = CreateRepository(production);
        var service = new ReportService(repository.Object);

        var response = await service.GetProductionRecordsAsync(new ProductionRecordFilter
        {
            Year = 2026,
            Paginate = false
        });

        var record = Assert.Single(response.Data);
        Assert.Equal(55, record.Times.Dandori);
        Assert.Equal(40, record.Times.Running);
    }

    [Fact]
    public async Task GetDandoriSummaryAsync_IncludesAllStatusesBeforeFirstRunningStatus()
    {
        var production = CreateProduction(
            (2, 5),
            (4, 10),
            (1, 15),
            (3, 10));
        var repository = CreateRepository(production);
        var service = new ReportService(repository.Object);

        var response = await service.GetDandoriSummaryAsync(2026);

        var operatorSummary = Assert.Single(response.Data.Operators);
        Assert.Equal(25, operatorSummary.Months.Jan);
        Assert.Equal(25, operatorSummary.Total!.Jan);
        Assert.Equal(1, operatorSummary.Count!.Jan);
    }

    [Fact]
    public async Task GetProductionRecordsAsync_CountsNonDandoriStatusesBeforeFirstRunningAsDandori()
    {
        var production = CreateProduction(
            (2, 5),
            (4, 10),
            (1, 15),
            (2, 5),
            (3, 10));
        var repository = CreateRepository(production);
        var service = new ReportService(repository.Object);

        var response = await service.GetProductionRecordsAsync(new ProductionRecordFilter
        {
            Year = 2026,
            Paginate = false
        });

        var record = Assert.Single(response.Data);
        Assert.Equal(25, record.Times.Dandori);
        Assert.Equal(20, record.Times.Running);
    }

    private static Mock<IReportRepository> CreateRepository(Production production)
    {
        var repository = new Mock<IReportRepository>();
        repository
            .Setup(x => x.GetProductionRecordsDataAsync(2026, null))
            .ReturnsAsync([production]);
        repository
            .Setup(x => x.GetOperatorsAsync())
            .ReturnsAsync([production.User]);

        return repository;
    }

    private static Production CreateProductionWithStatuses()
    {
        return CreateProduction(
            (3, 10),
            (2, 5),
            (3, 20),
            (2, 5),
            (3, 15),
            (1, 5),
            (4, 10),
            (2, 10),
            (1, 15));
    }

    private static Production CreateProduction(params (int Code, int DurationMinutes)[] statuses)
    {
        var machine = new Machine { Id = 1, Name = "Machine 1" };
        var user = new User { Id = 1, Name = "Operator 1", Role = "Operator" };
        var product = new Product
        {
            Id = 1,
            ProductNo = "P001",
            PartName = "Product 1",
            PartNo = "PART-001",
            Rpm = 10
        };
        var production = new Production
        {
            Id = 1,
            MachineId = machine.Id,
            Machine = machine,
            UserId = user.Id,
            User = user,
            ProductId = product.Id,
            Product = product,
            Quantity = 400,
            CreatedAt = new DateTime(2026, 1, 1, 10, 5, 0),
            UpdatedAt = new DateTime(2026, 1, 1, 23, 59, 0)
        };

        var nextCreatedAt = production.CreatedAt;
        production.Statuses = statuses
            .Select((status, index) =>
            {
                var entity = CreateStatus(
                    index + 1,
                    status.Code,
                    nextCreatedAt,
                    status.DurationMinutes,
                    production);
                nextCreatedAt = nextCreatedAt.AddMinutes(status.DurationMinutes);
                return entity;
            })
            .ToList();

        return production;
    }

    private static Status CreateStatus(
        int id,
        int code,
        DateTime createdAt,
        int durationMinutes,
        Production production)
    {
        return new Status
        {
            Id = id,
            MachineId = production.MachineId,
            Machine = production.Machine,
            ProductionId = production.Id,
            Production = production,
            Code = code,
            CreatedAt = createdAt,
            UpdatedAt = createdAt.AddMinutes(durationMinutes),
            Duration = durationMinutes * 60
        };
    }
}
