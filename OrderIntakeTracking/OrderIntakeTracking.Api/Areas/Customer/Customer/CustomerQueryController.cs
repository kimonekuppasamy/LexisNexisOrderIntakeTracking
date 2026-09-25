using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Customer.Customer
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<CustomersModel> _customers;
        private const string CacheKey = "customer";
        private static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

        private readonly IMemoryCache _cache;

        public CustomerQueryController(IFileReadWriteRepo<CustomersModel> customers, IMemoryCache cache)
        {
            _customers = customers;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<CustomersModel?>> GetCustomer(string email)
        {
            if (_cache.TryGetValue(CacheKey, out CustomersModel? customer) && customer != null && customer.Email == email)
            {
                return Ok(customer);
            }

            var customers = await _customers.GetAll();
            if(customers == null || !customers.Any())
            {
                return NotFound();
            }
            customer = customers.FirstOrDefault(c => c.Email == email);
            if(customer == null)
            {
                return NotFound();
            }
            _cache.Set(CacheKey, customer, Lifetime);
            return Ok(customer);
        }
    }
}
