using System;

namespace KcfMonitoringSystem.Application.Common;

public class ApiResponse<T>
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public PaginationMetadata? Pagination { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success", PaginationMetadata? pagination = null)
    {
        return new ApiResponse<T> { Status = true, Message = message, Data = data, Pagination = pagination };
    }

    public static ApiResponse<T> Error(string message)
    {
        return new ApiResponse<T> { Status = false, Message = message, Data = default };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public int Total { get; set; }
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
