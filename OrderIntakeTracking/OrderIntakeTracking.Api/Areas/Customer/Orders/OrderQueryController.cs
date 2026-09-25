using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Customer.Orders
{
    [ApiController]
    [Route("api/customer/orders")]
    public class OrderQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<OrderModel> _orders;

        public OrderQueryController(IFileReadWriteRepo<OrderModel> orders)
        {
            _orders = orders;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderModel>>> GetOrders(Guid customerId)
        {
            var orders = await _orders.GetAll();
            orders = orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
            return Ok(orders);
        }

        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<OrderModel>> GetOrderByOrderNumber(string orderNumber, Guid customerId)
        {
            var orders = await _orders.GetAll();
            var order = orders.FirstOrDefault(x => x.OrderNumber == orderNumber && x.CustomerId == customerId);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}
