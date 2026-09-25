using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Admin.Orders
{
    [ApiController]
    [Route("api/admin/orders")]
    public class OrderQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<OrderModel> _orders;

        public OrderQueryController(IFileReadWriteRepo<OrderModel> orders)
        {
            _orders = orders;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderModel>>> GetOrders()
        {
            var orders = await _orders.GetAll();

            return Ok(orders.OrderByDescending(x => x.OrderDate).ToList());
        }

        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<OrderModel>> GetOrderByOrderNumber(string orderNumber)
        {
            var orders = await _orders.GetAll();

            var order = orders.FirstOrDefault(x => x.OrderNumber == orderNumber);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}
