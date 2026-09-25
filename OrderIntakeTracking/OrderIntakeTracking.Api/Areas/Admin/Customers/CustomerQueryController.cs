using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Admin.Customers
{
    [ApiController]
    [Route("api/admin/customer")]
    public class CustomerQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<CustomersModel> _customers;

        public CustomerQueryController(IFileReadWriteRepo<CustomersModel> customers)
        {
            _customers = customers;
        }

        [HttpGet]
        public async Task<ActionResult<List<CustomersModel>>> GetItems() => Ok(await _customers.GetAll());
    }
}
