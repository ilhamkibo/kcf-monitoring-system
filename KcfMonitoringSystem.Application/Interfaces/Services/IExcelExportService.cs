using KcfMonitoringSystem.Application.Dtos;

namespace KcfMonitoringSystem.Application.Interfaces.Services;

public interface IExcelExportService
{
    byte[] GenerateReport(
        DandoriSummaryDto dandori,
        List<ProductionRecordDto> productionRecords,
        int? year);
}