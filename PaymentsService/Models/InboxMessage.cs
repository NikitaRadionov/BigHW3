namespace PaymentsService.Models
{
    public class InboxMessage
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public bool Processed { get; set; } = false;
    }
}