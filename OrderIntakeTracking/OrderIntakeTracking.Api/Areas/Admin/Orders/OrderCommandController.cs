using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Models.Enums;

namespace OrderIntakeTracking.Api.Areas.Admin.Orders
{
    [ApiController]
    [Route("api/admin/orders")]
    public class OrderCommandController : ControllerBase
    {
        private readonly IFileReadWriteRepo<OrderModel> _orders;

        public OrderCommandController(IFileReadWriteRepo<OrderModel> orders)
        {
            _orders = orders;
        }

        [HttpPut("{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            var orders = await _orders.GetAll();

            var order = orders.FirstOrDefault(x => x.OrderId == orderId);

            if (order == null)
                return NotFound();

            var (success, message) = ChangeStatus(order, status);

            if (!success)
                return BadRequest(message);

            await _orders.WriteAll(orders);

            return Ok();
        }

        private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
        {
            [OrderStatus.Pending] = new HashSet<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled },
            [OrderStatus.Confirmed] = new HashSet<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled },
            [OrderStatus.Shipped] = new HashSet<OrderStatus> { OrderStatus.Delivered },
            [OrderStatus.Delivered] = new HashSet<OrderStatus>(), // no transitions
            [OrderStatus.Cancelled] = new HashSet<OrderStatus>()  // no transitions
        };

        public static (bool Success, string Message) ChangeStatus(OrderModel order, OrderStatus newStatus)
        {
            var currentStatus = order.OrderStatus;

            if (currentStatus == newStatus)
            {
                return (false, $"Order is already {newStatus}.");
            }

            if (!AllowedTransitions[currentStatus].Contains(newStatus))
            {
                return (false, $"Cannot change order status from {currentStatus} to {newStatus}.");
            }

            order.OrderStatus = newStatus;

            if (newStatus == OrderStatus.Delivered || newStatus == OrderStatus.Cancelled)
            {
                order.OrderCompletedDate = DateTime.Now;
            }

            return (true, $"Order status changed from {currentStatus} to {newStatus}.");
        }
    }
}
