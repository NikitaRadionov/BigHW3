using System.ComponentModel.DataAnnotations;

namespace PaymentsService.DTOs
{
    public sealed class DepositDto
    {
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }
}