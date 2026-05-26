using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class ProductionEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public ProductionEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var productions = new List<ProductionDto> 
        { 
            new(1, 1, "Machine 1", 1, "User 1", 1, "PN001", "Part 1", 100, DateTime.Now) 
        };
        var response = ApiPagedResponse<List<ProductionDto>>.Ok(productions, "Success");
        
        _factory.ProductionServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<ProductionFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/productions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<ProductionDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }
}
