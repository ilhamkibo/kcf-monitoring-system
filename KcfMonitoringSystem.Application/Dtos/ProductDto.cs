using System.ComponentModel.DataAnnotations;

namespace KcfMonitoringSystem.Application.Dtos;

public record ProductDto(
    int Id,
    string ProductNo,
    string PartName,
    string PartNo,
    DateTime CreatedAt
);

public record CreateProductDto(
    [Required(ErrorMessage = "ProductNo is required.")]
    string ProductNo,

    [Required(ErrorMessage = "PartName is required.")]
    string PartName,

    [Required(ErrorMessage = "PartNo is required.")]
    string PartNo
);

public record UpdateProductDto(
    [Required(ErrorMessage = "ProductNo is required.")]
    string ProductNo,

    [Required(ErrorMessage = "PartName is required.")]
    string PartName,

    [Required(ErrorMessage = "PartNo is required.")]
    string PartNo
);
