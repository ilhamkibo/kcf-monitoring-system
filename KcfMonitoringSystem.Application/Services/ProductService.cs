using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiPagedResponse<List<ProductDto>>> GetAllAsync(ProductFilter filter)
    {
        var (products, totalCount) = await _repository.GetAllAsync(filter);

        var data = products.Select(x => new ProductDto(
            x.Id,
            x.ProductNo,
            x.PartName,
            x.PartNo,
            x.CreatedAt,
            x.Rpm,
            x.Customer
        )).ToList();

        PaginationMetadata? pagination = null;
        if (filter.Paginate == true)
        {
            pagination = new PaginationMetadata
            {
                Page = filter.Page,
                Limit = filter.Limit,
                Total = totalCount,
                TotalPages = filter.Limit > 0 ? (int)Math.Ceiling((double)totalCount / filter.Limit) : 0
            };
        }

        return ApiPagedResponse<List<ProductDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);

        if (product == null)
            return ApiResponse<ProductDto>.Error("Product not found");

        var data = new ProductDto(
            product.Id,
            product.ProductNo,
            product.PartName,
            product.PartNo,
            product.CreatedAt,
            product.Rpm,
            product.Customer
        );

        return ApiResponse<ProductDto>.Ok(data);
    }

    public async Task<ApiResponse<ProductDto>> GetByProductNoAsync(string productNo)
    {
        var product = await _repository.GetByProductNoAsync(productNo);

        if (product == null)
            return ApiResponse<ProductDto>.Error("Product not found");

        var data = new ProductDto(
            product.Id,
            product.ProductNo,
            product.PartName,
            product.PartNo,
            product.CreatedAt,
            product.Rpm,
            product.Customer
        );

        return ApiResponse<ProductDto>.Ok(data);
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto)
    {
        var existingProduct = await _repository.GetByProductNoAsync(dto.ProductNo);
        if (existingProduct != null)
            return ApiResponse<ProductDto>.Error("Product with this ProductNo already exists");

        var product = new Product
        {
            ProductNo = dto.ProductNo,
            PartName = dto.PartName,
            PartNo = dto.PartNo,
            Rpm = dto.Rpm,
            Customer = dto.Customer
        };

        var createdProduct = await _repository.AddAsync(product);

        var data = new ProductDto(
            createdProduct.Id,
            createdProduct.ProductNo,
            createdProduct.PartName,
            createdProduct.PartNo,
            createdProduct.CreatedAt,
            createdProduct.Rpm,
            createdProduct.Customer
        );

        return ApiResponse<ProductDto>.Ok(data, "Product created successfully");
    }


    public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Error("Product not found");

        var existingProduct = await _repository.GetByProductNoAsync(dto.ProductNo);
        if (existingProduct != null && existingProduct.Id != id)
            return ApiResponse<ProductDto>.Error("Another product with this ProductNo already exists");

        product.ProductNo = dto.ProductNo;
        product.PartName = dto.PartName;
        product.PartNo = dto.PartNo;
        product.Rpm = dto.Rpm;
        product.Customer = dto.Customer;
        product.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _repository.UpdateAsync(product);

        var data = new ProductDto(
            updatedProduct.Id,
            updatedProduct.ProductNo,
            updatedProduct.PartName,
            updatedProduct.PartNo,
            updatedProduct.CreatedAt,
            updatedProduct.Rpm,
            updatedProduct.Customer
        );

        return ApiResponse<ProductDto>.Ok(data, "Product updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<bool>.Error("Product not found");

        await _repository.DeleteAsync(product);

        return ApiResponse<bool>.Ok(true, "Product deleted successfully");
    }

}
