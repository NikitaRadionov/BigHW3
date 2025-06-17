namespace PaymentsService.Models
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }

        public DateTime OccurredOn { get; set; }
        public bool Sent { get; set; } = false;

        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}