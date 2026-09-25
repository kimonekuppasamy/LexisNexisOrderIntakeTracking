namespace OrderIntakeTracking.Api.Models
{
    public class CustomersModel
    {
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
