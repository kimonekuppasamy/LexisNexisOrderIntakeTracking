namespace OrderIntakeTracking.Api.Infrastructure.Storage
{
    /// <summary>Locations of the JSON data files under the app's content root.</summary>
    public static class DataFiles
    {
        public const string Folder = "Data";
        public const string Orders = "orders.json";
        public const string Products = "products.json";
        public const string Customers = "customers.json";

        public static string PathFor(string contentRootPath, string fileName) =>
            Path.Combine(contentRootPath, Folder, fileName);
    }
}
