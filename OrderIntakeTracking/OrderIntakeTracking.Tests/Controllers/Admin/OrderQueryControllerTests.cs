using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Tests.Infrastructure;
using AdminOrderQueryController = OrderIntakeTracking.Api.Areas.Admin.Orders.OrderQueryController;

namespace OrderIntakeTracking.Tests.Controllers.Admin
{
    public class OrderQueryControllerTests : IDisposable
    {
        private readonly TestDataStore _store = new();

        public void Dispose() => _store.Dispose();

        #region List orders, newest first

        [Fact]
        public async Task GetOrders_ReturnsOrdersNewestFirst()
        {
            var now = DateTime.UtcNow;
            var oldest = TestDataStore.Order(now.AddDays(-10));
            var newest = TestDataStore.Order(now);
            var middle = TestDataStore.Order(now.AddHours(-3));
            _store.SeedOrders(oldest, newest, middle);

            var orders = ActionResultAssert.Success<List<OrderModel>>(ActionResultAssert.Unwrap(await Controller().GetOrders()));

            Assert.Equal(new[] { newest.OrderId, middle.OrderId, oldest.OrderId }, orders.Select(o => o.OrderId));
        }

        [Fact]
        public async Task GetOrders_OrdersPlacedSecondsApart_AreStillNewestFirst()
        {
            var now = DateTime.UtcNow;
            var first = TestDataStore.Order(now.AddSeconds(-1));
            var second = TestDataStore.Order(now);
            _store.SeedOrders(first, second);

            var orders = ActionResultAssert.Success<List<OrderModel>>(ActionResultAssert.Unwrap(await Controller().GetOrders()));

            Assert.Equal(new[] { second.OrderId, first.OrderId }, orders.Select(o => o.OrderId));
        }

        [Fact]
        public async Task GetOrders_NoOrders_ReturnsEmptyList()
        {
            var orders = ActionResultAssert.Success<List<OrderModel>>(ActionResultAssert.Unwrap(await Controller().GetOrders()));

            Assert.Empty(orders);
        }

        #endregion

        #region Retrieve a single order

        [Fact]
        public async Task GetOrderByOrderNumber_ExistingOrder_ReturnsThatOrder()
        {
            var wanted = TestDataStore.Order(DateTime.UtcNow.AddHours(-1));
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow), wanted, TestDataStore.Order(DateTime.UtcNow.AddDays(-1)));

            var result = ActionResultAssert.Unwrap(await Controller().GetOrderByOrderNumber(wanted.OrderNumber));

            var order = ActionResultAssert.Success<OrderModel>(result);
            Assert.Equal(wanted.OrderId, order.OrderId);
            Assert.Equal(wanted.OrderNumber, order.OrderNumber);
        }

        [Theory]
        [InlineData("ORD-DOES-NOT-EXIST")]
        [InlineData("")]
        public async Task GetOrderByOrderNumber_UnknownOrder_ReturnsNotFound(string orderNumber)
        {
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow));

            var result = ActionResultAssert.Unwrap(await Controller().GetOrderByOrderNumber(orderNumber));

            Assert.True(ActionResultAssert.StatusCodeOf(result) == 404, $"Expected 404 Not Found but got {ActionResultAssert.Describe(result)}.");
        }

        #endregion

        private AdminOrderQueryController Controller() => new(_store.Orders);
    }
}
