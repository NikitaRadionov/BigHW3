namespace OrderService.DTOs.Messaging
{
    public class PaymentResultDto
    {
        public string OrderId { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? Reason { get; set; }
    }
}
