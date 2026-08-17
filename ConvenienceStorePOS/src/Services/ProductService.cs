using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<Product?> FindByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return await _productRepository.GetByCodeAsync(code);
        }

        public async Task<Product?> FindByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || category == "全て" || category == "All")
            {
                return await _productRepository.GetAllAsync();
            }
            return await _productRepository.GetByCategoryAsync(category);
        }

        public async Task<IReadOnlyList<Product>> SearchProductsAsync(string keyword, string? category = null)
        {
            return await _productRepository.SearchAsync(keyword, category);
        }

        public async Task<IReadOnlyList<string>> GetCategoriesAsync()
        {
            return await _productRepository.GetCategoriesAsync();
        }
    }
}
