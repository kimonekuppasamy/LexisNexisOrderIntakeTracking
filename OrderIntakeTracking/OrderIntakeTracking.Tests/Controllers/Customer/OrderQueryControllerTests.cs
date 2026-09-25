using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Tests.Infrastructure;
using CustomerOrderQueryController = OrderIntakeTracking.Api.Areas.Customer.Orders.OrderQueryController;

namespace OrderIntakeTracking.Tests.Controllers.Customer
{
    public class OrderQueryControllerTests : IDisposable
    {
        private readonly TestDataStore _store = new();

        public void Dispose() => _store.Dispose();

        [Fact]
        public async Task GetItems_ReturnsOnlyThatCustomersOrders_NewestFirst()
        {
            var customerId = Guid.NewGuid();
            var now = DateTime.UtcNow;
            var older = TestDataStore.Order(now.AddDays(-3), customerId: customerId);
            var newer = TestDataStore.Order(now.AddMinutes(-1), customerId: customerId);
            var someoneElses = TestDataStore.Order(now);
            _store.SeedOrders(older, someoneElses, newer);

            var orders = ActionResultAssert.Success<List<OrderModel>>(
                ActionResultAssert.Unwrap(await Controller().GetOrders(customerId)));

            Assert.Equal(new[] { newer.OrderId, older.OrderId }, orders.Select(o => o.OrderId));
        }

        [Fact]
        public async Task GetItems_CustomerWithNoOrders_ReturnsEmptyList()
        {
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow));

            var orders = ActionResultAssert.Success<List<OrderModel>>(
                ActionResultAssert.Unwrap(await Controller().GetOrders(Guid.NewGuid())));

            Assert.Empty(orders);
        }

        [Fact]
        public async Task GetOrderByOrderNumber_OwnOrder_ReturnsThatOrder()
        {
            var customerId = Guid.NewGuid();
            var wanted = TestDataStore.Order(DateTime.UtcNow, customerId: customerId);
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow.AddDays(-1), customerId: customerId), wanted);

            var result = ActionResultAssert.Unwrap(await Controller().GetOrderByOrderNumber(wanted.OrderNumber, customerId));

            Assert.Equal(wanted.OrderId, ActionResultAssert.Success<OrderModel>(result).OrderId);
        }

        [Fact]
        public async Task GetOrderByOrderNumber_AnotherCustomersOrder_ReturnsNotFound()
        {
            var someoneElses = TestDataStore.Order(DateTime.UtcNow);
            _store.SeedOrders(someoneElses);

            var result = ActionResultAssert.Unwrap(await Controller().GetOrderByOrderNumber(someoneElses.OrderNumber, Guid.NewGuid()));

            Assert.True(ActionResultAssert.StatusCodeOf(result) == 404, $"Expected 404 Not Found but got {ActionResultAssert.Describe(result)}.");
        }

        [Fact]
        public async Task GetOrderByOrderNumber_UnknownOrder_ReturnsNotFound()
        {
            var customerId = Guid.NewGuid();
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow, customerId: customerId));

            var result = ActionResultAssert.Unwrap(await Controller().GetOrderByOrderNumber("ORD-DOES-NOT-EXIST", customerId));

            Assert.True(ActionResultAssert.StatusCodeOf(result) == 404, $"Expected 404 Not Found but got {ActionResultAssert.Describe(result)}.");
        }

        private CustomerOrderQueryController Controller() => new(_store.Orders);
    }
}
