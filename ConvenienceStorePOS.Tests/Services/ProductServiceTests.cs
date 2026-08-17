using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using Moq;
using Xunit;

namespace ConvenienceStorePOS.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _service = new ProductService(_mockRepo.Object);
        }

        [Fact]
        public async Task FindByCodeAsync_CallsRepository_WhenCodeIsValid()
        {
            var product = new Product { Id = 1, Code = "12345", Name = "テスト商品", Price = 100m, TaxRateType = TaxRateType.Reduced8 };
            _mockRepo.Setup(r => r.GetByCodeAsync("12345")).ReturnsAsync(product);

            var result = await _service.FindByCodeAsync("12345");

            Assert.NotNull(result);
            Assert.Equal("テスト商品", result.Name);
            _mockRepo.Verify(r => r.GetByCodeAsync("12345"), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task FindByCodeAsync_ReturnsNull_ForEmptyOrWhitespaceCode(string? code)
        {
            var result = await _service.FindByCodeAsync(code!);

            Assert.Null(result);
            _mockRepo.Verify(r => r.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_All_DelegatesToGetAllAsync()
        {
            var list = new List<Product> { new() { Id = 1, Name = "A" } };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            var result = await _service.GetProductsByCategoryAsync("全て");

            Assert.Single(result);
            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
            _mockRepo.Verify(r => r.GetByCategoryAsync(It.IsAny<string>()), Times.Never);
        }
    }
}
