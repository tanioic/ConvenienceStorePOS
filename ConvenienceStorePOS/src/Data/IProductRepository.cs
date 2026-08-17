using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Data
{
    public interface IProductRepository
    {
        Task<Product?> GetByCodeAsync(string code);
        Task<Product?> GetByIdAsync(int id);
        Task<IReadOnlyList<Product>> GetAllAsync();
        Task<IReadOnlyList<Product>> GetByCategoryAsync(string category);
        Task<IReadOnlyList<Product>> SearchAsync(string keyword, string? category = null);
        Task<IReadOnlyList<string>> GetCategoriesAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
    }
}
