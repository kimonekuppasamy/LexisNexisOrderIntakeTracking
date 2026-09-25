using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Services;

namespace OrderIntakeTracking.Api.Areas.Customer.Orders
{
    [ApiController]
    [Route("api/customer/orders")]
    public class OrderCommandController : ControllerBase
    {
        private readonly OrderService _order;

        public OrderCommandController(OrderService order)
        {
            _order = order;
        }

        [HttpPost]
        public async Task<ActionResult<OrderModel?>> CreateOrder(CartModel cart)
        {
            var result = await _order.SubmitOrderAsync(cart);

            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Order);
        }
    }
}
