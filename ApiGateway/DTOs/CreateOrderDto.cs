using System.ComponentModel.DataAnnotations;

namespace ApiGateway.DTOs;

public sealed record CreateOrderDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }

    [Required]
    [StringLength(500)]
    public string Description { get; init; }
}