using Microsoft.AspNetCore.Mvc;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Api.Areas.Admin.Products
{
    [ApiController]
    [Route("api/admin/products")]
    public class ProductCommandController : ControllerBase
    {
        private readonly IFileReadWriteRepo<ProductsModel> _products;

        public ProductCommandController(IFileReadWriteRepo<ProductsModel> products)
        {
            _products = products;
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(Guid productId, UpdateProductModel updateProductModel)
        {
            if (updateProductModel.Quantity < 0 || updateProductModel.Price < 0)
            {
                return BadRequest("Quantity and Price must be non-negative.");
            }

            var products = await _products.GetAll();

            var product = products.FirstOrDefault(x => x.ProductId == productId);

            if (product == null)
                return NotFound();

            product.ProductQuantity = updateProductModel.Quantity;
            product.ProductPrice = updateProductModel.Price;

            await _products.WriteAll(products);

            return Ok();
        }
    }
}
