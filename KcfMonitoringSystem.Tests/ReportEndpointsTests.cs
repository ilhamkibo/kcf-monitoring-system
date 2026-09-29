using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class ReportEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public ReportEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDandori_ReturnsOk()
    {
        // Arrange
        var operatorDto = new DandoriOperatorDto(
            1,
            "Operator 1",
            new DandoriMonthsDto(null, null, null, null, null, 100, 200, null, null, null, null, null),
            150
        );
        var averageDto = new DandoriAverageDto(null, null, null, null, null, 100, 200, null, null, null, null, null, 150);
        var summary = new DandoriSummaryDto(new List<DandoriOperatorDto> { operatorDto }, averageDto);
        var response = ApiResponse<DandoriSummaryDto>.Ok(summary);

        _factory.ReportServiceMock
            .Setup(x => x.GetDandoriSummaryAsync(It.IsAny<int?>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/reports/dandori?year=2026");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<DandoriSummaryDto>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!.Operators);
        Assert.Equal("Operator 1", content.Data.Operators[0].Name);
        Assert.Equal(100, content.Data.Operators[0].Months.Jun);
        Assert.Equal(150, content.Data.Average.Overall);
    }

    [Fact]
    public async Task GetProductionRecords_ReturnsOk()
    {
        // Arrange
        var record = new ProductionRecordDto(
            "2026-08-01",
            new ProductionRecordMachineDto(1, "JBP-13"),
            new ProductionRecordItemDto(1, "22122-KWN-9010-Y1_JBP-13", "ROLLER WEIGHT"),
            new ProductionRecordSpeedDto(100, 6000),
            new List<ProductionRecordOperatorDto> { new(1, "NANA") },
            new ProductionRecordTimesDto(225, 2745),
            185133,
            67.44
        );
        var pagination = new PaginationMetadata { Page = 1, Limit = 10, Total = 1, TotalPages = 1 };
        var response = ApiPagedResponse<List<ProductionRecordDto>>.Ok(new List<ProductionRecordDto> { record }, "Success", pagination);

        _factory.ReportServiceMock
            .Setup(x => x.GetProductionRecordsAsync(It.IsAny<ProductionRecordFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/reports/production-records?year=2026");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<ProductionRecordDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
        Assert.Equal("2026-08-01", content.Data[0].Date);
        Assert.Equal("JBP-13", content.Data[0].Machine.Code);
        Assert.Equal(67.44, content.Data[0].OperatingRate);
        Assert.NotNull(content.Pagination);
        Assert.Equal(1, content.Pagination.Page);
    }
}

