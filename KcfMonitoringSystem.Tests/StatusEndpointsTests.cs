using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class StatusEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public StatusEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var statuses = new List<StatusDto> 
        { 
            new(1, 1, "Machine 1", 1, 1, "User 1", 1, "Part 1", "P001", DateTime.Now, DateTime.Now.AddMinutes(10), 600) 
        };
        var response = ApiPagedResponse<List<StatusDto>>.Ok(statuses, "Success");
        
        _factory.StatusServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/statuses");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<StatusDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }

    [Fact]
    public async Task GetTimeline_ReturnsOk()
    {
        // Arrange
        var timeline = new List<StatusTimelineDto> 
        { 
            new(1, "Machine 1", new List<TimelineDto> { new(DateTime.Now, DateTime.Now.AddMinutes(10), 1, 1, "User 1", 1, "Part 1", "P001") }) 
        };
        var response = ApiResponse<List<StatusTimelineDto>>.Ok(timeline);
        
        _factory.StatusServiceMock
            .Setup(x => x.GetTimelineAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/statuses/timeline");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<List<StatusTimelineDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }

    [Fact]
    public async Task GetActivity_ReturnsOk()
    {
        // Arrange
        var activity = new List<ActivityDto> 
        { 
            new(DateTime.Today, new List<ActivityDetailDto> { new("User 1", "Product 1", 3600, 1) }) 
        };
        var response = ApiResponse<List<ActivityDto>>.Ok(activity);
        
        _factory.StatusServiceMock
            .Setup(x => x.GetActivityAsync(It.IsAny<StatusFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/statuses/activity");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<List<ActivityDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }
}
