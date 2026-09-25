namespace OrderIntakeTracking.Api.Models
{
    public class OrderModel
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; } 
        public string CustomerName { get; set; } = string.Empty;
        public decimal OrderTotal { get; set; }
        public Enums.OrderStatus OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? OrderCompletedDate { get; set; }
        public List<OrderItemsModel>? OrderItems { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Guid CartId { get; set; }
    }
}
