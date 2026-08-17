using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using Moq;

namespace ConvenienceStorePOS.Tests.Services
{
    public class ProductServiceSpec005Tests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly ProductService _service;

        public ProductServiceSpec005Tests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _service = new ProductService(_mockRepo.Object);
        }

        // ========================================================
        // 5.3.1 AddProductAsync
        // ========================================================

        [Fact]
        public async Task AddProductAsync_ValidProduct_CallsRepository()
        {
            // Arrange
            var product = new Product { Code = "4901234567890", Name = "テスト商品", Price = 100, TaxRateType = TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };

            // Act
            await _service.AddProductAsync(product);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_NullProduct_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddProductAsync(null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddProductAsync_EmptyCode_ThrowsArgumentException(string? code)
        {
            // Arrange
            var product = new Product { Code = code!, Name = "テスト商品", Price = 100, TaxRateType = TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddProductAsync(product));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddProductAsync_EmptyName_ThrowsArgumentException(string? name)
        {
            // Arrange
            var product = new Product { Code = "4901234567890", Name = name!, Price = 100, TaxRateType = TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddProductAsync(product));
        }

        [Theory]
        [InlineData(-1)]
        public async Task AddProductAsync_NegativePrice_ThrowsArgumentException(decimal price)
        {
            // Arrange
            var product = new Product { Code = "4901234567890", Name = "テスト商品", Price = price, TaxRateType = TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddProductAsync(product));
        }

        // ========================================================
        // 5.3.2 UpdateProductAsync
        // ========================================================

        [Fact]
        public async Task UpdateProductAsync_ValidProduct_CallsRepository()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "4901234567890", Name = "更新後", Price = 200, TaxRateType = TaxRateType.Reduced8, Category = "食品", StockQuantity = 20 };

            // Act
            await _service.UpdateProductAsync(product);

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_NullProduct_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateProductAsync(null!));
        }

        [Fact]
        public async Task UpdateProductAsync_ZeroId_ThrowsArgumentException()
        {
            // Arrange
            var product = new Product { Id = 0, Code = "4901234567890", Name = "テスト", Price = 100, TaxRateType = TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProductAsync(product));
        }

        // ========================================================
        // 5.3.3 DeleteProductAsync
        // ========================================================

        [Fact]
        public async Task DeleteProductAsync_ValidId_CallsRepository()
        {
            // Act
            await _service.DeleteProductAsync(1);

            // Assert
            _mockRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteProductAsync_InvalidId_ThrowsArgumentException(int id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteProductAsync(id));
        }

        // ========================================================
        // 5.3.4 GetCategoriesAsync
        // ========================================================

        [Fact]
        public async Task GetCategoriesAsync_DelegatesToRepository()
        {
            // Arrange
            var categories = new List<string> { "飲料", "食品", "日用品" };
            _mockRepo.Setup(r => r.GetCategoriesAsync()).ReturnsAsync(categories);

            // Act
            var result = await _service.GetCategoriesAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains("飲料", result);
            _mockRepo.Verify(r => r.GetCategoriesAsync(), Times.Once);
        }
    }
}
