using KcfMonitoringSystem.Application.Common;
using KcfMonitoringSystem.Application.Dtos;
using KcfMonitoringSystem.Application.Filters;
using KcfMonitoringSystem.Application.Interfaces.Repositories;
using KcfMonitoringSystem.Application.Interfaces.Services;
using KcfMonitoringSystem.Domain.Entities;

namespace KcfMonitoringSystem.Application.Services;

public class ReportService : IReportService
{
    private readonly IReportRepository _repository;

    public ReportService(IReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<DandoriSummaryDto>> GetDandoriSummaryAsync(int? year)
    {
        var targetYear = year ?? DateTime.Now.Year;
        var productions = await _repository.GetProductionRecordsDataAsync(targetYear);
        var operators = await _repository.GetOperatorsAsync();

        var existingOpIds = operators.Select(o => o.Id).ToHashSet();
        foreach (var p in productions)
        {
            if (p.User != null && !existingOpIds.Contains(p.User.Id))
            {
                operators.Add(p.User);
                existingOpIds.Add(p.User.Id);
            }
        }
        operators = operators.OrderBy(o => o.Id).ToList();

        var groups = GroupContiguousProductions(productions);

        // Map operatorId -> month (1..12) -> list of dandori seconds per occurrence
        var opMonthDandori = new Dictionary<int, Dictionary<int, List<double>>>();
        foreach (var op in operators)
        {
            opMonthDandori[op.Id] = new Dictionary<int, List<double>>();
            for (int m = 1; m <= 12; m++)
            {
                opMonthDandori[op.Id][m] = new List<double>();
            }
        }

        foreach (var group in groups)
        {
            double dandoriSecs = CalculateStatusDurationSeconds(group.SelectMany(p => p.Statuses), 3);
            if (dandoriSecs <= 0) continue;

            int month = group[0].CreatedAt.Month;
            if (month < 1 || month > 12) continue;

            var groupOperators = group
                .Select(p => p.User)
                .Where(u => u != null)
                .GroupBy(u => u.Id)
                .Select(g => g.Key);

            foreach (var opId in groupOperators)
            {
                if (opMonthDandori.TryGetValue(opId, out var monthDict))
                {
                    monthDict[month].Add(dandoriSecs);
                }
            }
        }

        var operatorDtos = new List<DandoriOperatorDto>();
        var monthOperatorValues = new Dictionary<int, List<int>>();
        for (int m = 1; m <= 12; m++) monthOperatorValues[m] = new List<int>();

        foreach (var op in operators)
        {
            var monthVals = new int?[13];
            var monthTotalVals = new int?[13];
            var monthCountVals = new int?[13];
            var nonNullValues = new List<int>();

            for (int m = 1; m <= 12; m++)
            {
                var dandoriList = opMonthDandori[op.Id][m];
                if (dandoriList.Count > 0)
                {
                    // === DEBUG & DETAIL DATA ===
                    int totalMinutes = (int)Math.Round(dandoriList.Sum() / 60.0, MidpointRounding.AwayFromZero);
                    int count = dandoriList.Count;

                    // Opsi 1: Rata-rata dari total detik / count / 60
                    double avgMinutes = (dandoriList.Sum() / count) / 60.0;
                    int roundedAvg = (int)Math.Round(avgMinutes, MidpointRounding.AwayFromZero);

                    // Opsi 2 (jika ingin rata-rata langsung dari totalMinutes / count):
                    // int roundedAvg = (int)Math.Round((double)totalMinutes / count, MidpointRounding.AwayFromZero);

                    monthVals[m] = roundedAvg;
                    monthTotalVals[m] = totalMinutes;
                    monthCountVals[m] = count;

                    nonNullValues.Add(roundedAvg);
                    monthOperatorValues[m].Add(roundedAvg);
                }
                else
                {
                    monthVals[m] = null;
                    monthTotalVals[m] = null;
                    monthCountVals[m] = null;
                }
            }

            int? opAvg = nonNullValues.Count > 0
                ? (int)Math.Round(nonNullValues.Average(), MidpointRounding.AwayFromZero)
                : null;

            var monthsDto = new DandoriMonthsDto(
                monthVals[1], monthVals[2], monthVals[3], monthVals[4],
                monthVals[5], monthVals[6], monthVals[7], monthVals[8],
                monthVals[9], monthVals[10], monthVals[11], monthVals[12]
            );

            var totalMonthsDto = new DandoriMonthsDto(
                monthTotalVals[1], monthTotalVals[2], monthTotalVals[3], monthTotalVals[4],
                monthTotalVals[5], monthTotalVals[6], monthTotalVals[7], monthTotalVals[8],
                monthTotalVals[9], monthTotalVals[10], monthTotalVals[11], monthTotalVals[12]
            );

            var countMonthsDto = new DandoriMonthsDto(
                monthCountVals[1], monthCountVals[2], monthCountVals[3], monthCountVals[4],
                monthCountVals[5], monthCountVals[6], monthCountVals[7], monthCountVals[8],
                monthCountVals[9], monthCountVals[10], monthCountVals[11], monthCountVals[12]
            );

            operatorDtos.Add(new DandoriOperatorDto(op.Id, op.Name, monthsDto, opAvg, totalMonthsDto, countMonthsDto));
        }

        var rootMonthAvgs = new int?[13];
        var validMonthlyAverages = new List<int>();

        for (int m = 1; m <= 12; m++)
        {
            if (monthOperatorValues[m].Count > 0)
            {
                int monthAvg = (int)Math.Round(monthOperatorValues[m].Average(), MidpointRounding.AwayFromZero);
                rootMonthAvgs[m] = monthAvg;
                validMonthlyAverages.Add(monthAvg);
            }
            else
            {
                rootMonthAvgs[m] = null;
            }
        }

        int? overallAvg = validMonthlyAverages.Count > 0
            ? (int)Math.Round(validMonthlyAverages.Average(), MidpointRounding.AwayFromZero)
            : null;

        var rootAverageDto = new DandoriAverageDto(
            rootMonthAvgs[1], rootMonthAvgs[2], rootMonthAvgs[3], rootMonthAvgs[4],
            rootMonthAvgs[5], rootMonthAvgs[6], rootMonthAvgs[7], rootMonthAvgs[8],
            rootMonthAvgs[9], rootMonthAvgs[10], rootMonthAvgs[11], rootMonthAvgs[12],
            overallAvg
        );

        var summary = new DandoriSummaryDto(operatorDtos, rootAverageDto);
        return ApiResponse<DandoriSummaryDto>.Ok(summary);
    }

    public async Task<ApiPagedResponse<List<ProductionRecordDto>>> GetProductionRecordsAsync(ProductionRecordFilter filter)
    {
        var targetYear = filter.Year ?? DateTime.Now.Year;
        var productions = await _repository.GetProductionRecordsDataAsync(targetYear, filter.MachineId);

        var result = new List<ProductionRecordDto>();
        if (productions.Count == 0)
        {
            PaginationMetadata? emptyPagination = filter.Paginate == true
                ? new PaginationMetadata { Page = filter.Page, Limit = filter.Limit, Total = 0, TotalPages = 0 }
                : null;
            return ApiPagedResponse<List<ProductionRecordDto>>.Ok(result, "Success", emptyPagination);
        }

        var groups = GroupContiguousProductions(productions);
        foreach (var currentGroup in groups)
        {
            var first = currentGroup[0];
            var dateStr = first.CreatedAt.ToString("yyyy-MM-dd");
            var machineDto = new ProductionRecordMachineDto(first.Machine.Id, first.Machine.Name);

            var itemNo = first.Product?.PartNo ?? first.Product?.ProductNo ?? "";
            var itemName = first.Product?.PartName ?? "";
            var itemId = first.Product?.Id ?? 0;
            var itemDto = new ProductionRecordItemDto(itemId, itemNo, itemName);

            int rpm = first.Product?.Rpm ?? 0;
            var speedDto = new ProductionRecordSpeedDto(rpm, rpm * 60);

            var operators = currentGroup
                .Select(p => p.User)
                .Where(u => u != null)
                .GroupBy(u => u.Id)
                .Select(g => new ProductionRecordOperatorDto(g.Key, g.First().Name))
                .ToList();

            var allStatuses = currentGroup.SelectMany(p => p.Statuses).ToList();

            double dandoriSecs = CalculateStatusDurationSeconds(allStatuses, 3);
            double runningSecs = CalculateStatusDurationSeconds(allStatuses, 1, 2, 4);

            int dandoriMinutes = (int)Math.Round(dandoriSecs / 60.0, MidpointRounding.AwayFromZero);
            int runningMinutes = (int)Math.Round(runningSecs / 60.0, MidpointRounding.AwayFromZero);

            var timesDto = new ProductionRecordTimesDto(dandoriMinutes, runningMinutes);

            // long productQuantity = currentGroup.Sum(p => (long)(p.ActualQty > 0 ? p.ActualQty : p.Quantity));
            long productQuantity = currentGroup.Sum(p => (long)p.ActualQty);

            double operatingRate = 0;
            if (runningMinutes > 0 && rpm > 0)
            {
                operatingRate = Math.Round(((double)productQuantity / (rpm * runningMinutes)), 2);
                // operatingRate = Math.Round(((double)productQuantity / (rpm * runningMinutes)) * 100.0, 2);
            }

            result.Add(new ProductionRecordDto(
                dateStr,
                machineDto,
                itemDto,
                speedDto,
                operators,
                timesDto,
                productQuantity,
                operatingRate
            ));
        }

        int totalCount = result.Count;
        List<ProductionRecordDto> pagedResult;
        PaginationMetadata? pagination = null;

        if (filter.Paginate == true)
        {
            pagedResult = result.Skip((filter.Page - 1) * filter.Limit).Take(filter.Limit).ToList();
            pagination = new PaginationMetadata
            {
                Page = filter.Page,
                Limit = filter.Limit,
                Total = totalCount,
                TotalPages = filter.Limit > 0 ? (int)Math.Ceiling((double)totalCount / filter.Limit) : 0
            };
        }
        else
        {
            pagedResult = result;
        }

        return ApiPagedResponse<List<ProductionRecordDto>>.Ok(pagedResult, "Success", pagination);
    }

    private static List<List<Production>> GroupContiguousProductions(List<Production> productions)
    {
        var groups = new List<List<Production>>();
        int i = 0;
        while (i < productions.Count)
        {
            var first = productions[i];
            var currentGroup = new List<Production> { first };

            int j = i + 1;
            while (j < productions.Count &&
                   productions[j].MachineId == first.MachineId &&
                   productions[j].ProductId == first.ProductId)
            {
                currentGroup.Add(productions[j]);
                j++;
            }
            groups.Add(currentGroup);
            i = j;
        }
        return groups;
    }

    private static double CalculateStatusDurationSeconds(IEnumerable<Status> statuses, params int[] codes)
    {
        return statuses
            .Where(s => codes.Contains(s.Code))
            .Sum(s => s.Duration > 0
                ? (double)s.Duration
                : (s.UpdatedAt.HasValue ? (s.UpdatedAt.Value - s.CreatedAt).TotalSeconds : 0));
    }
}

