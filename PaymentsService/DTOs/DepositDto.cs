using System.ComponentModel.DataAnnotations;

namespace PaymentsService.DTOs;

public sealed record DepositDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; init; }
}