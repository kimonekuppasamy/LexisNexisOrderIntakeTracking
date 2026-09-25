using OrderIntakeTracking.Tests.Infrastructure;
using OrderIntakeTracking.Api.Areas.Admin.Products;
using OrderIntakeTracking.Api.Models;

namespace OrderIntakeTracking.Tests.Controllers.Admin
{
    public class ProductCommandControllerTests : IDisposable
    {
        private readonly TestDataStore _store = new();

        public void Dispose() => _store.Dispose();

        [Fact]
        public async Task UpdateProduct_ValidValues_ArePersisted()
        {
            // products.json is camelCase (as in the real Data folder)
            var product = TestDataStore.Product("FLW-001", 450.00m, stock: 25);
            _store.SeedProducts(product);

            var result = await UpdateProduct(product.ProductId, 10, 499.99m);

            Assert.True(ActionResultAssert.StatusCodeOf(result) is >= 200 and < 300, $"Got {ActionResultAssert.Describe(result)}.");
            var stored = Assert.Single(_store.ReadProducts());
            Assert.Equal(10, stored.ProductQuantity);
            Assert.Equal(499.99m, stored.ProductPrice);
        }

        [Fact]
        public async Task UpdateProduct_ZeroPrice_IsAllowed()
        {
            var product = TestDataStore.Product("FLW-001", 450.00m);
            _store.SeedProducts(product);

            var result = await UpdateProduct(product.ProductId, 5, 0.00m);

            Assert.True(ActionResultAssert.StatusCodeOf(result) is >= 200 and < 300, $"Got {ActionResultAssert.Describe(result)}.");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-100)]
        public async Task UpdateProduct_NegativePrice_ReturnsHelpfulBadRequestAndDoesNotPersist(double price)
        {
            var product = TestDataStore.Product("FLW-001", 450.00m);
            _store.SeedProducts(product);

            var result = await UpdateProduct(product.ProductId, 5, (decimal)price);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("price", message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(450.00m, Assert.Single(_store.ReadProducts()).ProductPrice);
        }

        [Fact]
        public async Task UpdateProduct_NegativeStockQuantity_ReturnsHelpfulBadRequest()
        {
            var product = TestDataStore.Product("FLW-001", 450.00m, stock: 25);
            _store.SeedProducts(product);

            var result = await UpdateProduct(product.ProductId, -1, 450.00m);

            var message = ActionResultAssert.HelpfulBadRequest(result);
            Assert.Contains("quantity", message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(25, Assert.Single(_store.ReadProducts()).ProductQuantity);
        }

        [Fact]
        public async Task UpdateProduct_UnknownProduct_ReturnsNotFound()
        {
            _store.SeedProducts(TestDataStore.Product("FLW-001", 450.00m));

            var result = await UpdateProduct(Guid.NewGuid(), 5, 10.00m);

            Assert.Equal(404, ActionResultAssert.StatusCodeOf(result));
        }

        private Task<Microsoft.AspNetCore.Mvc.IActionResult> UpdateProduct(Guid productId, int quantity, decimal price) =>
            new ProductCommandController(_store.Products)
                .UpdateProduct(productId, new UpdateProductModel { Quantity = quantity, Price = price });
    }
}
