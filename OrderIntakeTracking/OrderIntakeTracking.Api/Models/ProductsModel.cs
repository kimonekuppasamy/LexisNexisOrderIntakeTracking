namespace OrderIntakeTracking.Api.Models
{
    public class ProductsModel
    {
        public Guid ProductId { get; set; }
        public string ProductSKU { get; set; } = string.Empty;
        public string ProductName { get; set; } 
        public string ProductDescription { get; set; }
        public int ProductQuantity { get; set; }
        public decimal ProductPrice { get; set; }

    }

    public class UpdateProductModel
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
