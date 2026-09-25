using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Customer.Products
{
    [ApiController]
    [Route("api/customer/products")]
    public class ProductsQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<ProductsModel> _products;

        public ProductsQueryController(IFileReadWriteRepo<ProductsModel> products)
        {
            _products = products;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductsModel>>> GetItems() => Ok(await _products.GetAll());
    }
}
