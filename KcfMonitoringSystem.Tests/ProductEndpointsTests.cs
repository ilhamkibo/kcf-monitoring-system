using System.Net;
using System.Net.Http.Json;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using Moq;
using Xunit;

namespace KcfMonitoringSystem.Tests;

public class ProductEndpointsTests : IClassFixture<EndpointTestsBase>
{
    private readonly EndpointTestsBase _factory;
    private readonly HttpClient _client;

    public ProductEndpointsTests(EndpointTestsBase factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var products = new List<ProductDto> { new(1, "PN001", "Part 1", "P001", DateTime.Now) };
        var response = ApiPagedResponse<List<ProductDto>>.Ok(products, "Success");
        
        _factory.ProductServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<ProductFilter>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiPagedResponse<List<ProductDto>>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Single(content.Data!);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        // Arrange
        var product = new ProductDto(1, "PN001", "Part 1", "P001", DateTime.Now);
        var response = ApiResponse<ProductDto>.Ok(product);

        _factory.ProductServiceMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/products/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Equal("PN001", content.Data?.ProductNo);
    }

    [Fact]
    public async Task GetByProductNo_ReturnsOk()
    {
        // Arrange
        var product = new ProductDto(1, "PN001", "Part 1", "P001", DateTime.Now);
        var response = ApiResponse<ProductDto>.Ok(product);

        _factory.ProductServiceMock
            .Setup(x => x.GetByProductNoAsync("PN001"))
            .ReturnsAsync(response);

        // Act
        var result = await _client.GetAsync("/api/products/by-product-no/PN001");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var content = await result.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        Assert.NotNull(content);
        Assert.True(content.Status);
        Assert.Equal("PN001", content.Data?.ProductNo);
    }

    [Fact]
    public async Task Create_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateProductDto("PN002", "Part 2", "P002");
        var product = new ProductDto(2, "PN002", "Part 2", "P002", DateTime.Now);
        var response = ApiResponse<ProductDto>.Ok(product);

        _factory.ProductServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateProductDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PostAsJsonAsync("/api/products", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        // Arrange
        var dto = new UpdateProductDto("PN001-Updated", "Part 1 Updated", "P001-U");
        var product = new ProductDto(1, "PN001-Updated", "Part 1 Updated", "P001-U", DateTime.Now);
        var response = ApiResponse<ProductDto>.Ok(product);

        _factory.ProductServiceMock
            .Setup(x => x.UpdateAsync(1, It.IsAny<UpdateProductDto>()))
            .ReturnsAsync(response);

        // Act
        var result = await _client.PutAsJsonAsync("/api/products/1", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        // Arrange
        var response = ApiResponse<bool>.Ok(true, "Deleted");

        _factory.ProductServiceMock
            .Setup(x => x.DeleteAsync(1))
            .ReturnsAsync(response);

        // Act
        var result = await _client.DeleteAsync("/api/products/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}
