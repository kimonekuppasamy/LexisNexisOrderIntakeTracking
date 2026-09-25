using OrderIntakeTracking.Api.Infrastructure.Storage;
using OrderIntakeTracking.Api.Models;
using OrderIntakeTracking.Api.Services;

namespace OrderIntakeTracking.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOrderIntakeServices(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddJsonRepository<OrderModel>(DataFiles.Orders);
            services.AddJsonRepository<ProductsModel>(DataFiles.Products);
            services.AddJsonRepository<CustomersModel>(DataFiles.Customers);

            services.AddScoped<OrderService>();

            return services;
        }

        private static void AddJsonRepository<T>(this IServiceCollection services, string fileName) =>
            services.AddSingleton<IFileReadWriteRepo<T>>(provider =>
            {
                var contentRoot = provider.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
                return new FileReadWriteRepo<T>(DataFiles.PathFor(contentRoot, fileName));
            });
    }
}
