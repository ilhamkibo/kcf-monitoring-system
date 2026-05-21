using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KcfMonitoringSystem.Application.Common;

public class ApiResponse<T> where T : notnull
{
    [JsonPropertyOrder(1)]
    public bool Status { get; set; }

    [JsonPropertyOrder(2)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [JsonPropertyOrder(3)]
    public T Data { get; set; } = default!;

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T> { Status = true, Message = message, Data = data };
    }

    public static ApiResponse<T> Error(string message)
    {
        return new ApiResponse<T> { Status = false, Message = message, Data = default! };
    }
}

public class ApiPagedResponse<T> : ApiResponse<T> where T : notnull
{
    [Required]
    [JsonPropertyOrder(4)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetadata Pagination { get; set; } = null!;

    public static ApiPagedResponse<T> Ok(T data, string message = "Success", PaginationMetadata? pagination = null)
    {
        return new ApiPagedResponse<T> { Status = true, Message = message, Data = data, Pagination = pagination! };
    }
}

public class ApiErrorResponse
{
    [JsonPropertyOrder(1)]
    public bool Status { get; set; }

    [JsonPropertyOrder(2)]
    public string Message { get; set; } = string.Empty;

    [Required]
    [JsonPropertyOrder(3)]
    public System.Collections.Generic.Dictionary<string, string[]> Errors { get; set; } = new();

    public static ApiErrorResponse Create(string message, System.Collections.Generic.Dictionary<string, string[]>? errors = null)
    {
        return new ApiErrorResponse { Status = false, Message = message, Errors = errors ?? new() };
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


