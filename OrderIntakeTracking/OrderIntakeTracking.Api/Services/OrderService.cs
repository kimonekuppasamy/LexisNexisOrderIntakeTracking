using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Models.Enums;

namespace OrderIntakeTracking.Api.Services
{
    public record OrderResponse(OrderModel? Order, string? Error)
    {
        public bool Succeeded => Error == null;

        public static OrderResponse Success(OrderModel order) => new(order, null);
        public static OrderResponse Failure(string error) => new(null, error);
    }

    public class OrderService
    {
        private readonly IFileReadWriteRepo<OrderModel> _orders;
        private readonly IFileReadWriteRepo<ProductsModel> _products;
        private readonly IFileReadWriteRepo<CustomersModel> _customers;
        public OrderService(IFileReadWriteRepo<OrderModel> orders, IFileReadWriteRepo<ProductsModel> products, IFileReadWriteRepo<CustomersModel> customers)
        {
            _orders = orders;
            _products = products;
            _customers = customers;
        }

        public async Task<OrderResponse> SubmitOrderAsync(CartModel cart)
        {
            // validate
            if (cart == null || cart.CartProducts == null || !cart.CartProducts.Any())
            {

                return OrderResponse.Failure("Cart is invalid.");
            }

            if (cart.CustomerId == null)
            {
                return OrderResponse.Failure("Customer ID is required.");
            }

            if (cart.CartProducts.Any(x => x.Price < 0 || x.Quantity < 1))
            {
                return OrderResponse.Failure("Cart contains invalid product prices or quantities.");
            }

            var orders = await _orders.GetAll();

            if (orders.Any(x => x.CartId == cart.CartId && x.CustomerId == cart.CustomerId))
            {
                return OrderResponse.Failure("An order already exists for this cart.");
            }

            var products = await _products.GetAll();

            if (cart.CartProducts.Any(cp => !products.Any(p => p.ProductId == cp.ProductId)))
            {
                return OrderResponse.Failure("One or more products in the cart do not exist.");
            }

            var changedPrices = products
                .Where(p => cart.CartProducts.Any(cp => cp.ProductId == p.ProductId && cp.Price != p.ProductPrice))
                .Select(p => p.ProductName)
                .ToList();

            if (changedPrices.Any())
            {
                return OrderResponse.Failure($"The price has changed for: {string.Join(", ", changedPrices)}. Please review your cart.");
            }

            if (cart.CartProducts.Any(cp => products.Any(p => p.ProductQuantity <= 0 && p.ProductId == cp.ProductId)))
            {
                return OrderResponse.Failure("One or more products are out of stock.");
            }

            var customers = await _customers.GetAll();
            var customer = customers.FirstOrDefault(c => c.CustomerId == cart.CustomerId);
            if(customer == null)
            {
                return OrderResponse.Failure("Customer does not exist.");
            }

            var productsWithInsufficientStock = cart.CartProducts
                .Where(cp => products.Any(p => p.ProductId == cp.ProductId && p.ProductQuantity < cp.Quantity))
                .Select(cp => cp.ProductId)
                .ToList();

            if (productsWithInsufficientStock.Any())
            {
                var productNames = products
                    .Where(p => productsWithInsufficientStock.Contains(p.ProductId))
                    .Select(p => p.ProductName)
                    .ToList();
                return OrderResponse.Failure($"We do not have that many in stock for the following products: {string.Join(", ", productNames)}.");
            }

            var todayPrefix = $"ORD-{DateTime.Now:yyyyMMdd}-";
            var order = new OrderModel
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = $"{todayPrefix}{orders.Count(x => x.OrderNumber.StartsWith(todayPrefix)) + 1:D4}",
                OrderDate = DateTime.Now,
                OrderStatus = OrderStatus.Pending,
                CustomerId = cart.CustomerId!.Value,
                CartId = cart.CartId,
                CustomerName = customer.CustomerName
            };

            order.OrderItems = cart.CartProducts!
                .Select(item =>
                {
                    // price comes from the stored product, not the client
                    var product = products.First(x => x.ProductId == item.ProductId);
                    return new OrderItemsModel
                    {
                        OrderItemId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        Price = product.ProductPrice,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Product = product
                    };
                })
                .ToList();

            order.OrderTotal = order.OrderItems.Sum(x => x.Total);

            orders.Add(order);
            await _orders.WriteAll(orders);

            foreach(var product in order.OrderItems)
            {
                var findProduct = products.Where(x => x.ProductId == product.ProductId).First();
                findProduct.ProductQuantity = findProduct.ProductQuantity - product.Quantity;
            }
            await _products.WriteAll(products);

            return OrderResponse.Success(order);
        }
    }
}
