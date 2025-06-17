using System.ComponentModel.DataAnnotations;

namespace ApiGateway.DTOs;

public sealed record DepositDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }
}