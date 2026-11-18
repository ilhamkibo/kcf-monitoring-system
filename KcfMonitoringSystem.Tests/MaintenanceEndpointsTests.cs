using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;

namespace KcfMonitoringSystem.Tests;

public class MaintenanceEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public MaintenanceEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithFilters_ReturnsPagedStatuses()
    {
        var statuses = new List<MaintenanceDto>
        {
            new(
                1,
                10,
                "Machine 10",
                MaintenanceStatusCodes.UnderMaintenance,
                20,
                "Operator",
                30,
                "Part",
                "P001",
                new DateTime(2026, 11, 1),
                new DateTime(2026, 11, 1, 1, 0, 0),
                3600,
                100,
                [
                    new MaintenanceDetailDto(
                        1,
                        25,
                        "Maintenance Operator",
                        "Scheduled inspection",
                        new DateTime(2026, 11, 1),
                        new DateTime(2026, 11, 1, 1, 0, 0))
                ])
        };
        var pagination = new PaginationMetadata { Page = 2, Limit = 5, Total = 6, TotalPages = 2 };
        var response = ApiPagedResponse<List<MaintenanceDto>>.Ok(statuses, "Success", pagination);

        _factory.MaintenanceServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<MaintenanceFilter>()))
            .ReturnsAsync(response);

        var result = await _client.GetAsync(
            "/api/maintenance?page=2&limit=5&machineId=10&code=5&startDate=2026-11-01&endDate=2026-11-02");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<MaintenanceDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data);
        Assert.Equal(2, content.Pagination.Page);

        _factory.MaintenanceServiceMock.Verify(x => x.GetAllAsync(It.Is<MaintenanceFilter>(filter =>
            filter.Page == 2 &&
            filter.Limit == 5 &&
            filter.MachineId == 10 &&
            filter.Code == MaintenanceStatusCodes.UnderMaintenance &&
            filter.StartDate == new DateTime(2026, 11, 1) &&
            filter.EndDate == new DateTime(2026, 11, 2))), Times.Once);
    }
}
