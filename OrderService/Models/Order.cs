using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersService.Models
{
    public enum OrderStatusType
    {
        NEW,
        FINISHED,
        CANCELLED,
    }
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public OrderStatusType Status { get; set; } = OrderStatusType.NEW;
    }
}
