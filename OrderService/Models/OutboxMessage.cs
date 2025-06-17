namespace OrdersService.Models
{
    public enum OutboxMessageStatus
    {
        New,
        Processed,
        Failed
    }

    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public DateTime OccurredOn { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.New;
        public int RetryCount { get; set; } = 0;
        public DateTime? ProcessedAt { get; set; }
    }
}