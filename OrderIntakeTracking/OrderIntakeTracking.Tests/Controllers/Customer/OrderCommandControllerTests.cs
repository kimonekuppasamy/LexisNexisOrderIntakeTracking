using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Models.Enums;
using OrderIntakeTracking.Api.Services;
using OrderIntakeTracking.Tests.Infrastructure;
using CustomerOrderCommandController = OrderIntakeTracking.Api.Areas.Customer.Orders.OrderCommandController;

namespace OrderIntakeTracking.Tests.Controllers.Customer
{
    public class OrderCommandControllerTests : IDisposable
    {
        private readonly TestDataStore _store = new();
        private readonly Guid _customerId = Guid.NewGuid();
        private readonly ProductsModel _rose = TestDataStore.Product("FLW-001", 450.00m);
        private readonly ProductsModel _lily = TestDataStore.Product("FLW-002", 380.00m);
        private readonly ProductsModel _card = TestDataStore.Product("CRD-001", 19.99m);
        private readonly ProductsModel _freeGiftWrap = TestDataStore.Product("GFT-000", 0.00m);

        public OrderCommandControllerTests()
        {
            _store.SeedProducts(_rose, _lily, _card, _freeGiftWrap);
            _store.SeedCustomers(new CustomersModel { CustomerId = _customerId, CustomerName = "Test Customer", Email = "test.customer@example.com" });
        }

        public void Dispose() => _store.Dispose();

        #region Duplicate submissions (CartId is the client-provided external reference)

        [Fact]
        public async Task CreateOrder_SameCartIdSubmittedTwice_CreatesOnlyOneOrder()
        {
            var cartId = Guid.NewGuid();

            await CreateOrder(Cart(cartId, (_rose, 2)));
            await CreateOrder(Cart(cartId, (_rose, 2)));

            Assert.Single(_store.ReadOrders());
        }


        [Fact]
        public async Task CreateOrder_DifferentCartIds_CreateSeparateOrders()
        {
            var first = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart(Guid.NewGuid(), (_rose, 2))));
            var second = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart(Guid.NewGuid(), (_rose, 2))));

            Assert.NotEqual(first.OrderId, second.OrderId);
            Assert.Equal(2, _store.ReadOrders().Count);
        }

        [Fact]
        public async Task CreateOrder_CartIdIsStoredOnTheOrder()
        {
            var cartId = Guid.NewGuid();

            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart(cartId, (_rose, 2))));

            Assert.Equal(cartId, order.CartId);
            Assert.Equal(cartId, Assert.Single(_store.ReadOrders()).CartId);
        }

        #endregion

        #region Quantities are positive whole numbers; prices are non-negative

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public async Task CreateOrder_NonPositiveQuantity_ReturnsHelpfulBadRequestAndCreatesNoOrder(int quantity)
        {
            var result = await CreateOrder(Cart((_rose, 2), (_lily, quantity)));

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("quantit", message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_NegativePrice_ReturnsHelpfulBadRequestAndCreatesNoOrder()
        {
            var cart = Cart((_rose, 2));
            cart.CartProducts![0].Price = -10.00m;

            var result = await CreateOrder(cart);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("price", message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_CartPriceDiffersFromCurrentPrice_ReturnsHelpfulBadRequestNamingTheProduct()
        {
            var cart = Cart((_rose, 2));
            cart.CartProducts![0].Price = _rose.ProductPrice + 1;

            var result = await CreateOrder(cart);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains(_rose.ProductName, message);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_ZeroPricedProduct_IsAllowed()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_freeGiftWrap, 3))));

            Assert.Equal(0.00m, order.OrderTotal);
            Assert.Single(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_MinimumValidQuantityOfOne_IsAllowed()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 1))));

            Assert.Equal(1, Assert.Single(order.OrderItems!).Quantity);
        }

        [Fact]
        public async Task CreateOrder_NoCustomer_ReturnsHelpfulBadRequest()
        {
            var cart = Cart((_rose, 2));
            cart.CustomerId = null;

            var result = await CreateOrder(cart);

            ActionResultAssert.HelpfulBadRequest(result);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_ReturnsHelpfulBadRequest()
        {
            var result = await CreateOrder(Cart());

            ActionResultAssert.HelpfulBadRequest(result);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_NullCartProducts_ReturnsHelpfulBadRequest()
        {
            var cart = Cart();
            cart.CartProducts = null;

            var result = await CreateOrder(cart);

            ActionResultAssert.HelpfulBadRequest(result);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_OutOfStockProduct_ReturnsHelpfulBadRequest()
        {
            var soldOut = TestDataStore.Product("FLW-099", 120.00m, stock: 0);
            _store.SeedProducts(_rose, soldOut);

            var result = await CreateOrder(Cart((_rose, 2), (soldOut, 2)));

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("stock", message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_QuantityExceedsStock_ReturnsHelpfulBadRequestNamingTheProduct()
        {
            var scarce = TestDataStore.Product("FLW-050", 120.00m, stock: 3);
            _store.SeedProducts(_rose, scarce);

            var result = await CreateOrder(Cart((_rose, 2), (scarce, 5)));

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("stock", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(scarce.ProductName, message);
            Assert.DoesNotContain(_rose.ProductName, message);
            Assert.Empty(_store.ReadOrders());
        }

        [Fact]
        public async Task CreateOrder_QuantityEqualToStock_IsAllowed()
        {
            var scarce = TestDataStore.Product("FLW-050", 120.00m, stock: 3);
            _store.SeedProducts(scarce);

            var result = await CreateOrder(Cart((scarce, 3)));

            ActionResultAssert.Success<OrderModel>(result);
        }

        #endregion

        #region Line totals and overall totals are calculated on the server

        [Fact]
        public async Task CreateOrder_OrderTotal_IsSumOfPriceTimesQuantity()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 2), (_lily, 3))));

            Assert.Equal(2 * 450.00m + 3 * 380.00m, order.OrderTotal); // 2040.00
        }

        [Fact]
        public async Task CreateOrder_OrderTotal_KeepsDecimalPrecision()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_card, 3))));

            Assert.Equal(59.97m, order.OrderTotal);
        }

        [Fact]
        public async Task CreateOrder_EachLineHasServerCalculatedTotal()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 2), (_card, 3))));

            var rose = order.OrderItems!.Single(i => i.ProductId == _rose.ProductId);
            var card = order.OrderItems!.Single(i => i.ProductId == _card.ProductId);
            Assert.Equal(900.00m, rose.Total);
            Assert.Equal(59.97m, card.Total);
        }

        [Fact]
        public async Task CreateOrder_OrderTotal_EqualsSumOfLineItemTotals()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 2), (_lily, 2), (_card, 4))));

            var sumOfLines = order.OrderItems!.Sum(i => i.Total);
            Assert.Equal(sumOfLines, order.OrderTotal);
        }


        [Fact]
        public async Task CreateOrder_PersistedTotalMatchesReturnedTotal()
        {
            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 2), (_lily, 2))));

            var stored = Assert.Single(_store.ReadOrders());
            Assert.Equal(order.OrderId, stored.OrderId);
            Assert.Equal(1660.00m, stored.OrderTotal);
        }

        #endregion

        #region New order defaults

        [Fact]
        public async Task CreateOrder_NewOrder_StartsAsPendingForTheCustomer()
        {
            var before = DateTime.UtcNow;

            var order = ActionResultAssert.Success<OrderModel>(await CreateOrder(Cart((_rose, 2))));

            Assert.Equal(OrderStatus.Pending, order.OrderStatus);
            Assert.InRange(order.OrderDate.ToUniversalTime(), before, DateTime.UtcNow);
            Assert.Equal(_customerId, order.CustomerId);
            Assert.False(string.IsNullOrWhiteSpace(order.OrderNumber));
        }


        #endregion

        /// <summary>A new controller per call mirrors ASP.NET Core's per-request controller lifetime.</summary>
        private async Task<IActionResult> CreateOrder(CartModel cart)
        {
            if (ApiControllerModelValidation.Validate(cart, cart.CartProducts) is { } invalid)
                return invalid;

            var controller = new CustomerOrderCommandController(new OrderService(_store.Orders, _store.Products, _store.Customers));
            return ActionResultAssert.Unwrap(await controller.CreateOrder(cart));
        }

        /// <summary>Valid cart with a fresh CartId, priced from the catalog.</summary>
        private CartModel Cart(params (ProductsModel Product, int Quantity)[] lines) => Cart(Guid.NewGuid(), lines);

        private CartModel Cart(Guid cartId, params (ProductsModel Product, int Quantity)[] lines) => new()
        {
            CartId = cartId,
            CustomerId = _customerId,
            CartProducts = lines
                .Select(l => new CartProductModel { ProductId = l.Product.ProductId, Quantity = l.Quantity, Price = l.Product.ProductPrice })
                .ToList()
        };
    }
}
