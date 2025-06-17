namespace PaymentsService.DTOs.Messaging;

public class PaymentRequestDto
{
    public string OrderId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
}