using System.Text.Json;
using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Models.Enums;

namespace OrderIntakeTracking.Tests.Infrastructure
{
    /// <summary>
    /// Isolated, throw-away copy of the API's Data folder. Each test gets its own temp content root, and the
    /// repositories handed to controllers point at it, so the real OrderIntakeTracking.Api/Data files are never touched.
    /// Seeding and reading back go straight to the JSON files, independent of the repository under test.
    /// </summary>
    public sealed class TestDataStore : IDisposable
    {
        // Mirrors the casing of the real seed files: orders/customers are PascalCase, products are camelCase.
        private static readonly JsonSerializerOptions PascalCase = new() { WriteIndented = true };
        private static readonly JsonSerializerOptions CamelCase = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };

        public TestDataStore()
        {
            ContentRoot = Path.Combine(Path.GetTempPath(), "OrderIntakeTrackingTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(ContentRoot, DataFiles.Folder));

            SeedOrders();
            SeedProducts();
            SeedCustomers();
        }

        public string ContentRoot { get; }

        public IFileReadWriteRepo<OrderModel> Orders => Repository<OrderModel>(DataFiles.Orders);
        public IFileReadWriteRepo<ProductsModel> Products => Repository<ProductsModel>(DataFiles.Products);
        public IFileReadWriteRepo<CustomersModel> Customers => Repository<CustomersModel>(DataFiles.Customers);

        public void SeedOrders(params OrderModel[] orders) => Write(DataFiles.Orders, orders, PascalCase);
        public void SeedProducts(params ProductsModel[] products) => Write(DataFiles.Products, products, CamelCase);
        public void SeedCustomers(params CustomersModel[] customers) => Write(DataFiles.Customers, customers, PascalCase);

        public List<OrderModel> ReadOrders() => Read<OrderModel>(DataFiles.Orders);
        public List<ProductsModel> ReadProducts() => Read<ProductsModel>(DataFiles.Products);

        public static OrderModel Order(DateTime orderDate, OrderStatus status = OrderStatus.Pending, Guid? customerId = null) => new()
        {
            OrderId = Guid.NewGuid(),
            OrderNumber = $"ORD-{orderDate:yyyyMMddHHmmss}",
            CustomerId = customerId ?? Guid.NewGuid(),
            OrderDate = orderDate,
            OrderStatus = status,
            OrderItems = new List<OrderItemsModel>()
        };

        public static ProductsModel Product(string sku, decimal price, int stock = 100) => new()
        {
            ProductId = Guid.NewGuid(),
            ProductSKU = sku,
            ProductName = sku,
            ProductDescription = sku,
            ProductPrice = price,
            ProductQuantity = stock
        };

        private IFileReadWriteRepo<T> Repository<T>(string fileName) =>
            new FileReadWriteRepo<T>(DataFiles.PathFor(ContentRoot, fileName));

        private void Write<T>(string fileName, T[] items, JsonSerializerOptions options) =>
            File.WriteAllText(DataFiles.PathFor(ContentRoot, fileName), JsonSerializer.Serialize(items, options));

        private List<T> Read<T>(string fileName) =>
            JsonSerializer.Deserialize<List<T>>(File.ReadAllText(DataFiles.PathFor(ContentRoot, fileName)), ReadOptions)
            ?? new List<T>();

        public void Dispose()
        {
            try { Directory.Delete(ContentRoot, recursive: true); } catch (IOException) { }
        }
    }
}
