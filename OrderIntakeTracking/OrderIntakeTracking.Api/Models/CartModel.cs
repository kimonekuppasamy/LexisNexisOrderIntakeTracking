using System.ComponentModel.DataAnnotations.Schema;

namespace OrderIntakeTracking.Api.Models
{
    public class CartModel
    {
        public Guid CartId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CartProductModel>? CartProducts { get; set; }
    }

    public class CartProductModel
    {
        [ForeignKey("ProductsModel")]
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public virtual ProductsModel? Product { get; set; }
    }
}
