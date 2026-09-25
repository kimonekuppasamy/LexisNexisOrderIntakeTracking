using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Customer.Customer
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerCommandController : ControllerBase
    {
        private readonly IFileReadWriteRepo<CustomersModel> _customers;
        private const string CacheKey = "customer";
        private static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

        private readonly IMemoryCache _cache;

        public CustomerCommandController(IFileReadWriteRepo<CustomersModel> customers, IMemoryCache cache)
        {
            _customers = customers;
            _cache = cache;
        }

        [HttpPost]
        public async Task<ActionResult<CustomersModel?>> CreateCustomer(CustomersModel customer)
        {
            var customers = await _customers.GetAll();

            var existingCustomer = customers.FirstOrDefault(c => c.Email == customer.Email);

            if (existingCustomer != null)
            {
                _cache.Set(CacheKey, existingCustomer, Lifetime);
                return Ok(existingCustomer);
            }

            customers.Add(customer);
            await _customers.WriteAll(customers);

            _cache.Set(CacheKey, customer, Lifetime);
            return Ok(customer);
        }

        [HttpPost("logout")]
        public void Logout() => _cache.Remove(CacheKey);
    }
}
