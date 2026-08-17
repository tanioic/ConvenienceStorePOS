using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using Moq;
using Xunit;

namespace ConvenienceStorePOS.Tests.Services
{
    public class SaleServiceTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly SaleService _saleService;

        public SaleServiceTests()
        {
            _mockProductService = new Mock<IProductService>();
            _saleService = new SaleService(_mockProductService.Object);
        }

        [Fact]
        public void AddProduct_NewProduct_AddsToCartAndFiresEvent()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m, TaxRateType = TaxRateType.Reduced8 };
            bool eventFired = false;
            _saleService.CartChanged += (s, e) => eventFired = true;

            // Act
            var item = _saleService.AddProduct(product, 2);

            // Assert
            Assert.True(eventFired);
            Assert.Single(_saleService.Items);
            Assert.Equal(2, item.Quantity);
            Assert.Equal(150m, item.UnitPrice);
            Assert.Equal(300m, item.SubtotalExcludingTax);
        }

        [Fact]
        public void AddProduct_ExistingProduct_IncrementsQuantity()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m, TaxRateType = TaxRateType.Reduced8 };
            _saleService.AddProduct(product, 1);

            // Act
            var item = _saleService.AddProduct(product, 2);

            // Assert
            Assert.Single(_saleService.Items);
            Assert.Equal(3, item.Quantity);
        }

        [Fact]
        public async Task AddProductByCodeAsync_ValidCode_AddsProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m, TaxRateType = TaxRateType.Reduced8 };
            _mockProductService.Setup(p => p.FindByCodeAsync("111")).ReturnsAsync(product);

            // Act
            var item = await _saleService.AddProductByCodeAsync("111", 1);

            // Assert
            Assert.NotNull(item);
            Assert.Equal("おにぎり", item.ProductName);
            Assert.Single(_saleService.Items);
        }

        [Fact]
        public async Task AddProductByCodeAsync_InvalidCode_ReturnsNull()
        {
            // Arrange
            _mockProductService.Setup(p => p.FindByCodeAsync("999")).ReturnsAsync((Product?)null);

            // Act
            var item = await _saleService.AddProductByCodeAsync("999", 1);

            // Assert
            Assert.Null(item);
            Assert.Empty(_saleService.Items);
        }

        [Fact]
        public void IncrementAndDecrementQuantity_UpdatesCorrectly()
        {
            var product = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m };
            _saleService.AddProduct(product, 2);

            // Increment
            _saleService.IncrementQuantity(1, 1);
            Assert.Equal(3, _saleService.Items.First().Quantity);

            // Decrement
            _saleService.DecrementQuantity(1, 1);
            Assert.Equal(2, _saleService.Items.First().Quantity);

            // Decrement past 0 removes item
            _saleService.DecrementQuantity(1, 2);
            Assert.Empty(_saleService.Items);
        }

        [Fact]
        public void RemoveItem_RemovesProductFromCart()
        {
            var p1 = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m };
            var p2 = new Product { Id = 2, Code = "222", Name = "お茶", Price = 100m };
            _saleService.AddProduct(p1, 1);
            _saleService.AddProduct(p2, 1);

            var removed = _saleService.RemoveItem(1);

            Assert.True(removed);
            Assert.Single(_saleService.Items);
            Assert.Equal(2, _saleService.Items.First().ProductId);
        }

        [Fact]
        public void ClearCart_EmptiesAllItems()
        {
            var p1 = new Product { Id = 1, Code = "111", Name = "おにぎり", Price = 150m };
            _saleService.AddProduct(p1, 2);

            _saleService.ClearCart();

            Assert.Empty(_saleService.Items);
            Assert.Equal(0, _saleService.Summary.TotalQuantity);
            Assert.Equal(0m, _saleService.Summary.TotalAmount);
        }
    }
}
