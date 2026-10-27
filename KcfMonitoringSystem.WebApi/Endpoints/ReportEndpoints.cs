

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/dandori", async (IReportService reportService, [FromQuery] int? year = null) =>
        {
            var response = await reportService.GetDandoriSummaryAsync(year);
            return Results.Ok(response);
        }).Produces<ApiResponse<DandoriSummaryDto>>();

        group.MapGet("/production-records", async (
            IReportService reportService,
            [FromQuery] int? year = null,
            [FromQuery] int? machineId = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] bool paginate = true) =>
        {
            var filter = new ProductionRecordFilter
            {
                Year = year,
                MachineId = machineId,
                Page = page,
                Limit = limit,
                Paginate = paginate
            };
            var response = await reportService.GetProductionRecordsAsync(filter);
            return Results.Json(response, ProductionRecordJsonSerializerOptions);
        }).Produces<ApiPagedResponse<List<ProductionRecordDto>>>();

        // Export Excel endpoint
        group.MapGet("/export", async (
            IReportService reportService,
            IExcelExportService excelService,
            [FromQuery] int? year = null,
            [FromQuery] int? machineId = null) =>
        {
            // Get dandori data
            var dandoriResponse = await reportService.GetDandoriSummaryAsync(year);

            // Get all production records (no pagination for export)
            var recordsFilter = new ProductionRecordFilter
            {
                Year = year,
                MachineId = machineId,
                Paginate = false
            };
            var recordsResponse = await reportService.GetProductionRecordsAsync(recordsFilter);

            // Generate Excel
            var fileBytes = excelService.GenerateReport(
                dandoriResponse.Data,
                recordsResponse.Data,
                year);

            var fileName = $"KCF_Report_{year ?? DateTime.Now.Year}.xlsx";

            return Results.File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }).Produces<FileResult>();
    }

    private static readonly JsonSerializerOptions ProductionRecordJsonSerializerOptions = CreateProductionRecordJsonSerializerOptions();

    private static JsonSerializerOptions CreateProductionRecordJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new FourDecimalDoubleJsonConverter());

        return options;
    }

    private sealed class FourDecimalDoubleJsonConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.ToString("F4", CultureInfo.InvariantCulture));
        }
    }
}
