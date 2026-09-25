using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Models.Enums;
using OrderIntakeTracking.Tests.Infrastructure;
using AdminOrderCommandController = OrderIntakeTracking.Api.Areas.Admin.Orders.OrderCommandController;

namespace OrderIntakeTracking.Tests.Controllers.Admin
{
    public class OrderCommandControllerTests : IDisposable
    {
        private readonly TestDataStore _store = new();

        public void Dispose() => _store.Dispose();

        public static TheoryData<OrderStatus, OrderStatus> AllowedTransitions => new()
        {
            { OrderStatus.Pending, OrderStatus.Confirmed },
            { OrderStatus.Pending, OrderStatus.Cancelled },
            { OrderStatus.Confirmed, OrderStatus.Shipped },
            { OrderStatus.Confirmed, OrderStatus.Cancelled },
            { OrderStatus.Shipped, OrderStatus.Delivered },
        };

        public static TheoryData<OrderStatus, OrderStatus> DisallowedTransitions => new()
        {
            // Skipping steps
            { OrderStatus.Pending, OrderStatus.Shipped },
            { OrderStatus.Pending, OrderStatus.Delivered },
            { OrderStatus.Confirmed, OrderStatus.Delivered },
            // Going backwards
            { OrderStatus.Confirmed, OrderStatus.Pending },
            { OrderStatus.Shipped, OrderStatus.Pending },
            { OrderStatus.Shipped, OrderStatus.Confirmed },
            { OrderStatus.Delivered, OrderStatus.Shipped },
            // Cancelling once it has left the building
            { OrderStatus.Shipped, OrderStatus.Cancelled },
            { OrderStatus.Delivered, OrderStatus.Cancelled },
            // Terminal states
            { OrderStatus.Delivered, OrderStatus.Pending },
            { OrderStatus.Delivered, OrderStatus.Confirmed },
            { OrderStatus.Cancelled, OrderStatus.Pending },
            { OrderStatus.Cancelled, OrderStatus.Confirmed },
            { OrderStatus.Cancelled, OrderStatus.Shipped },
            { OrderStatus.Cancelled, OrderStatus.Delivered },
        };

        [Theory]
        [MemberData(nameof(AllowedTransitions))]
        public async Task UpdateOrderStatus_AllowedTransition_SucceedsAndIsPersisted(OrderStatus from, OrderStatus to)
        {
            var order = TestDataStore.Order(DateTime.UtcNow, from);
            _store.SeedOrders(order);

            var result = await Controller().UpdateOrderStatus(order.OrderId, to);

            Assert.True(ActionResultAssert.StatusCodeOf(result) is >= 200 and < 300,
                $"{from} -> {to} should be allowed but got {ActionResultAssert.Describe(result)}.");
            Assert.Equal(to, Assert.Single(_store.ReadOrders()).OrderStatus);
        }

        [Theory]
        [MemberData(nameof(DisallowedTransitions))]
        public async Task UpdateOrderStatus_DisallowedTransition_ReturnsHelpfulBadRequestNamingBothStatuses(OrderStatus from, OrderStatus to)
        {
            var order = TestDataStore.Order(DateTime.UtcNow, from);
            _store.SeedOrders(order);

            var result = await Controller().UpdateOrderStatus(order.OrderId, to);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains(from.ToString(), message);
            Assert.Contains(to.ToString(), message);
        }

        [Theory]
        [MemberData(nameof(DisallowedTransitions))]
        public async Task UpdateOrderStatus_DisallowedTransition_LeavesStoredStatusUnchanged(OrderStatus from, OrderStatus to)
        {
            var order = TestDataStore.Order(DateTime.UtcNow, from);
            _store.SeedOrders(order);

            await Controller().UpdateOrderStatus(order.OrderId, to);

            Assert.Equal(from, Assert.Single(_store.ReadOrders()).OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Pending)]
        [InlineData(OrderStatus.Confirmed)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Delivered)]
        [InlineData(OrderStatus.Cancelled)]
        public async Task UpdateOrderStatus_ToSameStatus_ReturnsHelpfulBadRequest(OrderStatus status)
        {
            var order = TestDataStore.Order(DateTime.UtcNow, status);
            _store.SeedOrders(order);

            var result = await Controller().UpdateOrderStatus(order.OrderId, status);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("already", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateOrderStatus_UndefinedStatusValue_ReturnsHelpfulBadRequest()
        {
            var order = TestDataStore.Order(DateTime.UtcNow, OrderStatus.Pending);
            _store.SeedOrders(order);

            var result = await Controller().UpdateOrderStatus(order.OrderId, (OrderStatus)99);

            ActionResultAssert.HelpfulBadRequest(result);
            Assert.Equal(OrderStatus.Pending, Assert.Single(_store.ReadOrders()).OrderStatus);
        }

        [Fact]
        public async Task UpdateOrderStatus_UnknownOrder_ReturnsNotFound()
        {
            _store.SeedOrders(TestDataStore.Order(DateTime.UtcNow, OrderStatus.Pending));

            var result = await Controller().UpdateOrderStatus(Guid.NewGuid(), OrderStatus.Confirmed);

            Assert.Equal(404, ActionResultAssert.StatusCodeOf(result));
        }

        [Fact]
        public async Task UpdateOrderStatus_OnlyChangesTheTargetedOrder()
        {
            var target = TestDataStore.Order(DateTime.UtcNow, OrderStatus.Pending);
            var other = TestDataStore.Order(DateTime.UtcNow.AddMinutes(-5), OrderStatus.Pending);
            _store.SeedOrders(target, other);

            await Controller().UpdateOrderStatus(target.OrderId, OrderStatus.Confirmed);

            var stored = _store.ReadOrders();
            Assert.Equal(OrderStatus.Confirmed, stored.Single(o => o.OrderId == target.OrderId).OrderStatus);
            Assert.Equal(OrderStatus.Pending, stored.Single(o => o.OrderId == other.OrderId).OrderStatus);
        }

        [Fact]
        public async Task UpdateOrderStatus_FullHappyPath_PendingToDelivered()
        {
            var order = TestDataStore.Order(DateTime.UtcNow, OrderStatus.Pending);
            _store.SeedOrders(order);

            foreach (var next in new[] { OrderStatus.Confirmed, OrderStatus.Shipped, OrderStatus.Delivered })
            {
                var result = await Controller().UpdateOrderStatus(order.OrderId, next);
                Assert.True(ActionResultAssert.StatusCodeOf(result) is >= 200 and < 300, $"Step to {next} failed: {ActionResultAssert.Describe(result)}.");
            }

            Assert.Equal(OrderStatus.Delivered, Assert.Single(_store.ReadOrders()).OrderStatus);
        }

        [Theory]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
        public async Task UpdateOrderStatus_ToTerminalStatus_SetsOrderCompletedDate(OrderStatus from, OrderStatus to)
        {
            var order = TestDataStore.Order(DateTime.UtcNow.AddDays(-2), from);
            _store.SeedOrders(order);
            var before = DateTime.UtcNow;

            await Controller().UpdateOrderStatus(order.OrderId, to);

            var completed = Assert.Single(_store.ReadOrders()).OrderCompletedDate;
            Assert.NotNull(completed);
            Assert.InRange(completed!.Value.ToUniversalTime(), before, DateTime.UtcNow);
        }

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
        [InlineData(OrderStatus.Confirmed, OrderStatus.Shipped)]
        public async Task UpdateOrderStatus_ToNonTerminalStatus_LeavesOrderCompletedDateEmpty(OrderStatus from, OrderStatus to)
        {
            var order = TestDataStore.Order(DateTime.UtcNow.AddDays(-2), from);
            _store.SeedOrders(order);

            await Controller().UpdateOrderStatus(order.OrderId, to);

            Assert.Null(Assert.Single(_store.ReadOrders()).OrderCompletedDate);
        }

        [Fact]
        public async Task UpdateOrderStatus_DisallowedTransition_DoesNotSetOrderCompletedDate()
        {
            var order = TestDataStore.Order(DateTime.UtcNow.AddDays(-2), OrderStatus.Shipped);
            _store.SeedOrders(order);

            await Controller().UpdateOrderStatus(order.OrderId, OrderStatus.Cancelled);

            Assert.Null(Assert.Single(_store.ReadOrders()).OrderCompletedDate);
        }

        private AdminOrderCommandController Controller() => new(_store.Orders);
    }
}
