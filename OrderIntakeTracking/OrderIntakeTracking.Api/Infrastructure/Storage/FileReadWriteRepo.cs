using System.Text.Json;

namespace OrderIntakeTracking.Api.Infrastructure.Storage
{
    public interface IFileReadWriteRepo<T>
    {
        Task<List<T>> GetAll();
        Task WriteAll(List<T> items);
    }
    public class FileReadWriteRepo<T> : IFileReadWriteRepo<T>
    {
        private static readonly JsonSerializerOptions ReadOptions = new() { PropertyNameCaseInsensitive = true };
        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        private readonly string _filePath;

        public FileReadWriteRepo(string filePath)
        {
            _filePath = filePath;
        }

        public async Task<List<T>> GetAll()
        {
            var json = await File.ReadAllTextAsync(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            return JsonSerializer.Deserialize<List<T>>(json, ReadOptions)
                   ?? new List<T>();
        }

        public async Task WriteAll(List<T> items)
        {
            var json = JsonSerializer.Serialize(items, WriteOptions);

            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
