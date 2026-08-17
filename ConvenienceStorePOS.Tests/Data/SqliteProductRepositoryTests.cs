using System.IO;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using Xunit;

namespace ConvenienceStorePOS.Tests.Data
{
    public class SqliteProductRepositoryTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly SqliteDatabaseInitializer _initializer;
        private readonly SqliteProductRepository _repository;

        public SqliteProductRepositoryTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"pos_test_{Guid.NewGuid():N}.db");
            _initializer = new SqliteDatabaseInitializer(_testDbPath);
            _repository = new SqliteProductRepository(_initializer);
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public async Task InitializeAndSeed_LoadsSeedProducts()
        {
            // Act
            await _initializer.InitializeAsync();
            var products = await _repository.GetAllAsync();

            // Assert
            Assert.NotEmpty(products);
            Assert.Contains(products, p => p.Name.Contains("熟成紅しゃけ"));
            Assert.Contains(products, p => p.Category == "飲料");
            Assert.Contains(products, p => p.Category == "日用品");
        }

        [Fact]
        public async Task GetByCodeAsync_ReturnsCorrectProduct()
        {
            // Arrange
            await _initializer.InitializeAsync();

            // Act
            var product = await _repository.GetByCodeAsync("4901001000018");

            // Assert
            Assert.NotNull(product);
            Assert.Equal("手巻おにぎり 熟成紅しゃけ", product.Name);
            Assert.Equal(160m, product.Price);
            Assert.Equal(TaxRateType.Reduced8, product.TaxRateType);
        }

        [Fact]
        public async Task GetByCodeAsync_NonExistentCode_ReturnsNull()
        {
            // Arrange
            await _initializer.InitializeAsync();

            // Act
            var product = await _repository.GetByCodeAsync("9999999999999");

            // Assert
            Assert.Null(product);
        }

        [Fact]
        public async Task SearchAsync_FindsByNameAndCategory()
        {
            // Arrange
            await _initializer.InitializeAsync();

            // Act
            var results = await _repository.SearchAsync("チキン");

            // Assert
            Assert.NotEmpty(results);
            Assert.All(results, p => Assert.True(p.Name.Contains("チキン") || p.Code.Contains("チキン")));
        }

        [Fact]
        public async Task AddAsync_InsertsNewProduct()
        {
            // Arrange
            await _initializer.InitializeAsync();
            var newProduct = new Product
            {
                Code = "4901009999999",
                Name = "新商品 プレミアムスイーツ",
                Price = 300m,
                TaxRateType = TaxRateType.Reduced8,
                Category = "菓子・デザート",
                StockQuantity = 50,
                IsActive = true
            };

            // Act
            await _repository.AddAsync(newProduct);
            var fetched = await _repository.GetByCodeAsync("4901009999999");

            // Assert
            Assert.NotNull(fetched);
            Assert.True(fetched.Id > 0);
            Assert.Equal("新商品 プレミアムスイーツ", fetched.Name);
        }
    }
}
