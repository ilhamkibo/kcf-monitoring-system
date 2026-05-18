using System;
using KcfMonitoringSystem.Domain.Entities;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Repositories;

public interface IProductionRepository
{
    Task<(List<Production> Data, int TotalCount)> GetAllAsync(ProductionFilter filter);
    Task<Production?> GetByIdAsync(int id);
}
