using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;

namespace KcfMonitoringSystem.Application.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly IMaintenanceRepository _repository;

    public MaintenanceService(IMaintenanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiPagedResponse<List<MaintenanceDto>>> GetAllAsync(MaintenanceFilter filter)
    {
        ValidateCode(filter.Code);

        var (start, end) = DateFilterHelper.Normalize(filter.StartDate, filter.EndDate);
        filter.StartDate = start;
        filter.EndDate = end;

        var (statuses, totalCount) = await _repository.GetAllAsync(filter);
        var data = statuses.Select(x => new MaintenanceDto(
            x.Id,
            x.MachineId,
            x.Machine.Name,
            x.Code,
            x.Production.UserId,
            x.Production.User.Name,
            x.Production.ProductId,
            x.Production.Product?.PartName,
            x.Production.Product?.PartNo,
            x.CreatedAt,
            x.UpdatedAt,
            x.Duration,
            x.Qty,
            x.Maintenances
                .OrderBy(m => m.CreatedAt)
                .ThenBy(m => m.Id)
                .Select(m => new MaintenanceDetailDto(
                    m.Id,
                    m.MaintenanceOperatorId,
                    m.MaintenanceOperator?.Name,
                    m.Reason,
                    m.CreatedAt,
                    m.UpdatedAt
                ))
                .ToList()
        )).ToList();

        PaginationMetadata? pagination = null;
        if (filter.Paginate == true)
        {
            pagination = new PaginationMetadata
            {
                Page = filter.Page,
                Limit = filter.Limit,
                Total = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / filter.Limit)
            };
        }

        return ApiPagedResponse<List<MaintenanceDto>>.Ok(data, "Success", pagination);
    }

    private static void ValidateCode(int? code)
    {
        if (!code.HasValue || MaintenanceStatusCodes.IsValid(code.Value))
            return;

        throw new ValidationException(
            "Code",
            $"Code must be {MaintenanceStatusCodes.UnderMaintenance} (under maintenance) or " +
            $"{MaintenanceStatusCodes.PreventiveMaintenance} (preventive maintenance).");
    }
}
