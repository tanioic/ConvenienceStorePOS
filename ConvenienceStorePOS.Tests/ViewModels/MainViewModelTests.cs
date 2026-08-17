using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using ConvenienceStorePOS.ViewModels;
using Moq;
using Xunit;

namespace ConvenienceStorePOS.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly Mock<ISaleService> _mockSaleService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IDatabaseInitializer> _mockDbInitializer;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockSaleService = new Mock<ISaleService>();
            _mockProductService = new Mock<IProductService>();
            _mockDbInitializer = new Mock<IDatabaseInitializer>();

            _mockSaleService.Setup(s => s.Items).Returns(new List<CartItem>());
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary());

            _viewModel = new MainViewModel(
                _mockSaleService.Object,
                _mockProductService.Object,
                _mockDbInitializer.Object
            );
        }

        [Fact]
        public async Task RegisterByBarcodeAsync_Success_UpdatesStatusAndClearsInput()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "4901001000018", Name = "熟成紅しゃけ", Price = 160m, TaxRateType = TaxRateType.Reduced8 };
            var cartItem = new CartItem(product, 1);

            _viewModel.BarcodeInput = "4901001000018";
            _mockSaleService.Setup(s => s.AddProductByCodeAsync("4901001000018", 1)).ReturnsAsync(cartItem);

            // Act
            await _viewModel.RegisterByBarcodeAsync();

            // Assert
            Assert.Equal(string.Empty, _viewModel.BarcodeInput);
            Assert.False(_viewModel.IsStatusError);
            Assert.Contains("熟成紅しゃけ", _viewModel.StatusMessage);
            _mockSaleService.Verify(s => s.AddProductByCodeAsync("4901001000018", 1), Times.Once);
        }

        [Fact]
        public async Task RegisterByBarcodeAsync_NotFound_SetsStatusError()
        {
            // Arrange
            _viewModel.BarcodeInput = "9999999999999";
            _mockSaleService.Setup(s => s.AddProductByCodeAsync("9999999999999", 1)).ReturnsAsync((CartItem?)null);

            // Act
            await _viewModel.RegisterByBarcodeAsync();

            // Assert
            Assert.Equal(string.Empty, _viewModel.BarcodeInput);
            Assert.True(_viewModel.IsStatusError);
            Assert.Contains("見つかりませんでした", _viewModel.StatusMessage);
        }

        [Fact]
        public void SelectProduct_AddsToSaleService()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "4901001000018", Name = "熟成紅しゃけ", Price = 160m };
            var productVm = new ProductItemViewModel(product);
            var cartItem = new CartItem(product, 1);
            _mockSaleService.Setup(s => s.AddProduct(product, 1)).Returns(cartItem);

            // Act
            _viewModel.SelectProduct(productVm);

            // Assert
            _mockSaleService.Verify(s => s.AddProduct(product, 1), Times.Once);
            Assert.Contains("熟成紅しゃけ", _viewModel.StatusMessage);
        }

        [Fact]
        public void ClearCart_CallsSaleServiceClearCart()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "テスト商品", Price = 100m };
            var cartItem = new CartItem(product, 1);
            _mockSaleService.Setup(s => s.Items).Returns(new List<CartItem> { cartItem });
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary(new List<CartItem> { cartItem }));

            // Trigger cart change so ViewModel has items
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            // Act
            _viewModel.ClearCart();

            // Assert
            _mockSaleService.Verify(s => s.ClearCart(), Times.Once);
            Assert.Contains("全取消", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task InitializeAsync_LoadsCategoriesAndProducts()
        {
            // Arrange
            _mockProductService.Setup(p => p.GetCategoriesAsync()).ReturnsAsync(new List<string> { "おにぎり・弁当", "飲料" });
            _mockProductService.Setup(p => p.SearchProductsAsync("", "全て")).ReturnsAsync(new List<Product>
            {
                new() { Id = 1, Name = "おにぎり" },
                new() { Id = 2, Name = "お茶" }
            });

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            _mockDbInitializer.Verify(d => d.InitializeAsync(), Times.Once);
            Assert.Equal(3, _viewModel.Categories.Count); // "全て" + 2 categories
            Assert.Equal(2, _viewModel.DisplayProducts.Count);
        }
    }
}
