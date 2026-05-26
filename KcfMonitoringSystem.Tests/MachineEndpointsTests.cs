using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class MachineEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public MachineEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var machines = new List<MachineDto> { new(1, "Machine 1", DateTime.Now) };
        var response = ApiPagedResponse<List<MachineDto>>.Ok(machines, "Success");
        
        _factory.MachineServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<MachineFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/machines");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<MachineDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        // Arrange
        var machine = new MachineDto(1, "Machine 1", DateTime.Now);
        var response = ApiResponse<MachineDto>.Ok(machine);

        _factory.MachineServiceMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/machines/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<MachineDto>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Equal("Machine 1", content.Data?.Name);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateMachineDto("New Machine");
        var machine = new MachineDto(1, "New Machine", DateTime.Now);
        var response = ApiResponse<MachineDto>.Ok(machine);

        _factory.MachineServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateMachineDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PostAsJsonAsync("/api/machines", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateMachineDto("Updated Machine");
        var machine = new MachineDto(1, "Updated Machine", DateTime.Now);
        var response = ApiResponse<MachineDto>.Ok(machine);

        _factory.MachineServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateMachineDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PutAsJsonAsync("/api/machines/1", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        // Arrange
        var response = ApiResponse<object>.Ok(true, "Deleted");

        _factory.MachineServiceMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.DeleteAsync("/api/machines/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
