using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IReportService
{
    Task<ApiResponse<DandoriSummaryDto>> GetDandoriSummaryAsync(int? year);
    Task<ApiPagedResponse<List<ProductionRecordDto>>> GetProductionRecordsAsync(ProductionRecordFilter filter);
}

