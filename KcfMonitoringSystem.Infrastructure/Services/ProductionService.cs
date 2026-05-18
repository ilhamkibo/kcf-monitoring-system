using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Common;

namespace KcfMonitoringSystem.Infrastructure;

public class ProductionService : IProductionService
{
    private readonly IProductionRepository _repository;

    public ProductionService(IProductionRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<ApiResponse<List<ProductionDto>>> GetAllAsync(ProductionFilter filter)
    {
        var (productions, totalCount) = await _repository.GetAllAsync(filter);

        var data = productions.Select(x => new ProductionDto(
            x.Id,
            x.MachineId,
            x.Machine?.Name ?? "",
            x.UserId,
            x.User?.Name ?? "",
            x.Quantity,
            x.CreatedAt
        )).ToList();

        var pagination = new PaginationMetadata
        {
            Page = filter.Page,
            Limit = filter.Limit,
            Total = totalCount,
            TotalPages = filter.Limit > 0 ? (int)Math.Ceiling((double)totalCount / filter.Limit) : 0
        };

        return ApiResponse<List<ProductionDto>>.Ok(data, "Success", pagination);
    }

    public async Task<ApiResponse<ProductionDto>> GetByIdAsync(int id)
    {
        var production = await _repository.GetByIdAsync(id);

        if (production == null)
            return ApiResponse<ProductionDto>.Error("Production not found");

        var data = new ProductionDto(
            production.Id,
            production.MachineId,
            production.Machine?.Name ?? "",
            production.UserId,
            production.User?.Name ?? "",
            production.Quantity,
            production.CreatedAt
        );

        return ApiResponse<ProductionDto>.Ok(data);
    }
}
