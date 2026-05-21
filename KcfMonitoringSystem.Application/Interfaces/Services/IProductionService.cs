using System;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Common;

namespace KcfMonitoringSystem.Application.Services;

public interface IProductionService
{
    Task<ApiPagedResponse<List<ProductionDto>>> GetAllAsync(ProductionFilter filter);
}
