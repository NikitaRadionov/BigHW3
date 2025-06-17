using System.ComponentModel.DataAnnotations;

namespace OrderService.Contracts.DTOs;

public sealed record CreateOrderDto(
    [property: Range(0.01, double.MaxValue)] decimal Amount,
    [property: Required, StringLength(500)] string Description
);