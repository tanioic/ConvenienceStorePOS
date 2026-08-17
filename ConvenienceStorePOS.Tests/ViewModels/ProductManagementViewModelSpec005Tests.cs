using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using ConvenienceStorePOS.ViewModels;
using Moq;

namespace ConvenienceStorePOS.Tests.ViewModels
{
    public class ProductManagementViewModelSpec005Tests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly ProductManagementViewModel _viewModel;

        public ProductManagementViewModelSpec005Tests()
        {
            _mockProductService = new Mock<IProductService>();
            _viewModel = new ProductManagementViewModel(_mockProductService.Object);
        }

        // ========================================================
        // 5.4.1 InitializeAsync
        // ========================================================

        [Fact]
        public async Task InitializeAsync_LoadsProductsAndCategories()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Code = "4901234567890", Name = "商品A", Price = 100, TaxRateType = Common.TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 },
                new Product { Id = 2, Code = "4901234567891", Name = "商品B", Price = 200, TaxRateType = Common.TaxRateType.Reduced8, Category = "食品", StockQuantity = 5 }
            };
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>())).ReturnsAsync(products);
            _mockProductService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<string> { "飲料", "食品" });

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            Assert.Equal(2, _viewModel.Products.Count);
            Assert.Contains("全て", _viewModel.Categories);
            Assert.Contains("飲料", _viewModel.Categories);
            Assert.Contains("食品", _viewModel.Categories);
        }

        // ========================================================
        // 5.4.2 Search
        // ========================================================

        [Fact]
        public async Task SearchAsync_FiltersByKeyword()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Code = "4901234567890", Name = "コカコーラ", Price = 150, TaxRateType = Common.TaxRateType.Standard10, Category = "飲料", StockQuantity = 20 }
            };
            _mockProductService.Setup(s => s.SearchProductsAsync("コーラ", It.IsAny<string?>())).ReturnsAsync(products);
            _mockProductService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<string>());

            _viewModel.SearchKeyword = "コーラ";

            // Act
            await _viewModel.SearchCommand.ExecuteAsync(null);

            // Assert
            Assert.Single(_viewModel.Products);
            Assert.Equal("コカコーラ", _viewModel.Products[0].Name);
        }

        [Fact]
        public async Task ClearSearchCommand_ResetsKeywordAndReloads()
        {
            // Arrange
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>()))
                               .ReturnsAsync(new List<Product>());
            _mockProductService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(new List<string>());

            _viewModel.SearchKeyword = "テスト";

            // Act
            await _viewModel.ClearSearchCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(string.Empty, _viewModel.SearchKeyword);
        }

        // ========================================================
        // 5.4.3 Add / Edit
        // ========================================================

        [Fact]
        public void StartAddNewCommand_SetsIsEditingTrue()
        {
            // Act
            _viewModel.StartAddNewCommand.Execute(null);

            // Assert
            Assert.True(_viewModel.IsEditing);
            Assert.Null(_viewModel.SelectedProduct);
        }

        [Fact]
        public void CancelEditCommand_ResetsEditingState()
        {
            // Arrange
            _viewModel.StartAddNewCommand.Execute(null);

            // Act
            _viewModel.CancelEditCommand.Execute(null);

            // Assert
            Assert.False(_viewModel.IsEditing);
            Assert.Null(_viewModel.SelectedProduct);
        }

        [Fact]
        public async Task SaveProductAsync_NewProduct_CallsAddProductAsync()
        {
            // Arrange
            _mockProductService.Setup(s => s.AddProductAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>()))
                               .ReturnsAsync(new List<Product>());

            _viewModel.StartAddNewCommand.Execute(null);
            _viewModel.EditCode = "4901234567890";
            _viewModel.EditName = "新商品";
            _viewModel.EditPrice = 100;
            _viewModel.EditCategory = "飲料";
            _viewModel.EditStockQuantity = 10;

            // Act
            await _viewModel.SaveProductCommand.ExecuteAsync(null);

            // Assert
            _mockProductService.Verify(s => s.AddProductAsync(It.Is<Product>(p =>
                p.Code == "4901234567890" && p.Name == "新商品" && p.Price == 100)), Times.Once);
            Assert.False(_viewModel.IsEditing);
        }

        [Fact]
        public async Task SaveProductAsync_ExistingProduct_CallsUpdateProductAsync()
        {
            // Arrange
            var existing = new ProductItemViewModel(new Product { Id = 1, Code = "4901234567890", Name = "既存商品", Price = 100, TaxRateType = Common.TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 });
            _mockProductService.Setup(s => s.UpdateProductAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>()))
                               .ReturnsAsync(new List<Product>());

            _viewModel.StartEditCommand.Execute(existing);

            // Act
            await _viewModel.SaveProductCommand.ExecuteAsync(null);

            // Assert
            _mockProductService.Verify(s => s.UpdateProductAsync(It.Is<Product>(p => p.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task SaveProductAsync_EmptyCode_SetsErrorStatus()
        {
            // Arrange
            _viewModel.StartAddNewCommand.Execute(null);
            _viewModel.EditCode = "";
            _viewModel.EditName = "テスト";

            // Act
            await _viewModel.SaveProductCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.IsStatusError);
            Assert.Contains("コード", _viewModel.StatusMessage);
            _mockProductService.Verify(s => s.AddProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task SaveProductAsync_EmptyName_SetsErrorStatus()
        {
            // Arrange
            _viewModel.StartAddNewCommand.Execute(null);
            _viewModel.EditCode = "4901234567890";
            _viewModel.EditName = "";

            // Act
            await _viewModel.SaveProductCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.IsStatusError);
            Assert.Contains("商品名", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task SaveProductAsync_NegativePrice_SetsErrorStatus()
        {
            // Arrange
            _viewModel.StartAddNewCommand.Execute(null);
            _viewModel.EditCode = "4901234567890";
            _viewModel.EditName = "テスト";
            _viewModel.EditPrice = -1;

            // Act
            await _viewModel.SaveProductCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.IsStatusError);
            Assert.Contains("単価", _viewModel.StatusMessage);
        }

        // ========================================================
        // 5.4.4 Delete
        // ========================================================

        [Fact]
        public async Task DeleteProductAsync_ValidItem_CallsServiceAndRefreshes()
        {
            // Arrange
            var product = new Product { Id = 1, Code = "4901234567890", Name = "削除商品", Price = 100, TaxRateType = Common.TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 };
            _mockProductService.Setup(s => s.DeleteProductAsync(1)).Returns(Task.CompletedTask);
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>()))
                               .ReturnsAsync(new List<Product>());

            var item = new ProductItemViewModel(product);

            // Act
            await _viewModel.DeleteProductCommand.ExecuteAsync(item);

            // Assert
            _mockProductService.Verify(s => s.DeleteProductAsync(1), Times.Once);
            Assert.Contains("削除", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task DeleteProductAsync_NullItem_DoesNothing()
        {
            // Act
            await _viewModel.DeleteProductCommand.ExecuteAsync(null);

            // Assert
            _mockProductService.Verify(s => s.DeleteProductAsync(It.IsAny<int>()), Times.Never);
        }

        // ========================================================
        // 5.4.5 Edge Cases
        // ========================================================

        [Fact]
        public void Constructor_NullService_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ProductManagementViewModel(null!));
        }

        [Fact]
        public void HasSelectedProduct_FalseWhenNull()
        {
            // Assert
            Assert.False(_viewModel.HasSelectedProduct);
        }

        [Fact]
        public void HasSelectedProduct_TrueWhenSelected()
        {
            // Arrange
            var product = new ProductItemViewModel(new Product { Id = 1, Code = "4901234567890", Name = "商品", Price = 100, TaxRateType = Common.TaxRateType.Standard10, Category = "飲料", StockQuantity = 10 });

            // Act
            _viewModel.SelectedProduct = product;

            // Assert
            Assert.True(_viewModel.HasSelectedProduct);
        }

        [Fact]
        public async Task SelectCategoryAsync_UpdatesSelectedCategory()
        {
            // Arrange
            _mockProductService.Setup(s => s.SearchProductsAsync(It.IsAny<string>(), It.IsAny<string?>()))
                               .ReturnsAsync(new List<Product>());

            // Act
            await _viewModel.SelectCategoryCommand.ExecuteAsync("飲料");

            // Assert
            Assert.Equal("飲料", _viewModel.SelectedCategory);
        }
    }
}
