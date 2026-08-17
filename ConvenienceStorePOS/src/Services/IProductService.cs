using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public interface IProductService
    {
        Task<Product?> FindByCodeAsync(string code);
        Task<Product?> FindByIdAsync(int id);
        Task<IReadOnlyList<Product>> GetAllProductsAsync();
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(string category);
        Task<IReadOnlyList<Product>> SearchProductsAsync(string keyword, string? category = null);
        Task<IReadOnlyList<string>> GetCategoriesAsync();
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
