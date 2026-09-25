using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Admin.Products
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductQueryController : ControllerBase
    {
        private readonly IFileReadWriteRepo<ProductsModel> _products;

        public ProductQueryController(IFileReadWriteRepo<ProductsModel> products)
        {
            _products = products;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductsModel>>> GetItems() => Ok(await _products.GetAll());
    }
}
