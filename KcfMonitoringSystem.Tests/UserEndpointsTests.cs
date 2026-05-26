using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class UserEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public UserEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var users = new List<UserDto> { new(1, "User 1", "user1@test.com", "user1", "Admin", "Group 1", "Machine 1", DateTime.Now) };
        var response = ApiPagedResponse<List<UserDto>>.Ok(users, "Success");
        
        _factory.UserServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<UserFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<UserDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        // Arrange
        var user = new UserDto(1, "User 1", "user1@test.com", "user1", "Admin", "Group 1", "Machine 1", DateTime.Now);
        var response = ApiResponse<UserDto>.Ok(user);

        _factory.UserServiceMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/users/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Equal("User 1", content.Data?.Name);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateUserDto("New User", "new@test.com", "newuser", "User", 1, 1);
        var user = new UserDto(1, "New User", "new@test.com", "newuser", "User", "Group 1", "Machine 1", DateTime.Now);
        var response = ApiResponse<UserDto>.Ok(user);

        _factory.UserServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PostAsJsonAsync("/api/users", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateUserDto("Updated User", "updated@test.com", "updateduser", "User", 1, 1);
        var user = new UserDto(1, "Updated User", "updated@test.com", "updateduser", "User", "Group 1", "Machine 1", DateTime.Now);
        var response = ApiResponse<UserDto>.Ok(user);

        _factory.UserServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateUserDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PutAsJsonAsync("/api/users/1", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        // Arrange
        var response = ApiResponse<object>.Ok(true, "Deleted");

        _factory.UserServiceMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.DeleteAsync("/api/users/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
