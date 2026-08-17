using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using ConvenienceStorePOS.ViewModels;
using Moq;
using Xunit;

namespace ConvenienceStorePOS.Tests.ViewModels
{
    /// <summary>
    /// SPEC-002: 売上集計・明細確認
    /// MainViewModel のサマリー自動更新・CanOpenAccounting 制御を検証するテスト
    /// </summary>
    public class SaleSummaryViewModelSpec002Tests
    {
        private readonly Mock<ISaleService> _mockSaleService;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IAccountingService> _mockAccountingService;
        private readonly Mock<IReceiptService> _mockReceiptService;
        private readonly Mock<IDatabaseInitializer> _mockDbInitializer;
        private readonly MainViewModel _viewModel;

        public SaleSummaryViewModelSpec002Tests()
        {
            _mockSaleService = new Mock<ISaleService>();
            _mockProductService = new Mock<IProductService>();
            _mockAccountingService = new Mock<IAccountingService>();
            _mockReceiptService = new Mock<IReceiptService>();
            _mockDbInitializer = new Mock<IDatabaseInitializer>();

            _mockSaleService.Setup(s => s.Items).Returns(new List<CartItem>());
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary());
            _mockAccountingService
                .Setup(a => a.CalculateCurrencyBreakdown(It.IsAny<decimal>()))
                .Returns((decimal amount) => new CurrencyBreakdown(amount));

            _viewModel = new MainViewModel(
                _mockSaleService.Object,
                _mockProductService.Object,
                _mockAccountingService.Object,
                _mockReceiptService.Object,
                _mockDbInitializer.Object
            );
        }

        // =====================================================
        // 2.4 会計ボタンの制御 (CanOpenAccounting)
        // =====================================================

        [Fact]
        public void CanOpenAccounting_EmptyCart_IsFalse()
        {
            // Initial state: empty cart, TotalAmount = 0
            Assert.False(_viewModel.CanOpenAccounting);
            Assert.False(_viewModel.HasCartItems);
        }

        [Fact]
        public void CanOpenAccounting_CartHasItemsAndPositiveTotal_IsTrue()
        {
            // Arrange: Simulate a cart with 1 item
            var product = new Product { Id = 1, Name = "おにぎり", Price = 160m, TaxRateType = TaxRateType.Reduced8 };
            var cartItem = new CartItem(product, 1);
            var summary = new SaleSummary(new List<CartItem> { cartItem });

            _mockSaleService.Setup(s => s.Items).Returns(new List<CartItem> { cartItem });
            _mockSaleService.Setup(s => s.Summary).Returns(summary);

            // Act: Fire CartChanged event to trigger ViewModel update
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            // Assert
            Assert.True(_viewModel.HasCartItems);
            Assert.True(_viewModel.CanOpenAccounting);
        }

        [Fact]
        public void OpenAccounting_EmptyCart_SetsStatusError()
        {
            // Empty cart state
            _viewModel.OpenAccounting();

            Assert.True(_viewModel.IsStatusError);
            Assert.False(_viewModel.IsAccountingModalOpen);
        }

        // =====================================================
        // 2.5 カート変更検知と自動更新 (Cart Change Detection)
        // =====================================================

        [Fact]
        public void CartChanged_Event_UpdatesSummaryProperties()
        {
            // Arrange: Create a summary with mixed taxes
            var onigiri = new Product { Id = 1, Name = "おにぎり", Price = 160m, TaxRateType = TaxRateType.Reduced8 };
            var umbrella = new Product { Id = 2, Name = "傘", Price = 650m, TaxRateType = TaxRateType.Standard10 };

            var items = new List<CartItem>
            {
                new(onigiri, 2),
                new(umbrella, 1)
            };
            var summary = new SaleSummary(items);

            _mockSaleService.Setup(s => s.Items).Returns(items);
            _mockSaleService.Setup(s => s.Summary).Returns(summary);

            // Act: Raise CartChanged
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            // Assert: ViewModel summary properties are updated
            Assert.Equal(3, _viewModel.TotalQuantity);               // 2 + 1
            Assert.Equal(970m, _viewModel.SubtotalExcludingTax);     // 320 + 650
            Assert.Equal(320m, _viewModel.Reduced8TaxableAmount);
            Assert.Equal(25m, _viewModel.Reduced8TaxAmount);         // Floor(320 * 0.08) = 25
            Assert.Equal(650m, _viewModel.Standard10TaxableAmount);
            Assert.Equal(65m, _viewModel.Standard10TaxAmount);       // Floor(650 * 0.10) = 65
            Assert.Equal(90m, _viewModel.TotalTaxAmount);            // 25 + 65
            Assert.Equal(1060m, _viewModel.TotalAmount);             // 970 + 90
        }

        [Fact]
        public void CartChanged_ClearCart_ResetsAllSummaryPropertiesToZero()
        {
            // Arrange: First add a product
            var product = new Product { Id = 1, Name = "お茶", Price = 130m, TaxRateType = TaxRateType.Reduced8 };
            var items = new List<CartItem> { new(product, 1) };
            var nonEmptySummary = new SaleSummary(items);

            _mockSaleService.Setup(s => s.Items).Returns(items);
            _mockSaleService.Setup(s => s.Summary).Returns(nonEmptySummary);
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            // Verify non-empty
            Assert.True(_viewModel.HasCartItems);

            // Act: Simulate cart clear
            _mockSaleService.Setup(s => s.Items).Returns(new List<CartItem>());
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary());
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            // Assert: All zeroed out
            Assert.Equal(0, _viewModel.TotalQuantity);
            Assert.Equal(0m, _viewModel.TotalAmount);
            Assert.False(_viewModel.HasCartItems);
            Assert.False(_viewModel.CanOpenAccounting);
        }

        [Fact]
        public void CartChanged_MultipleEvents_AlwaysReflectsLatestSummary()
        {
            // First event: 1 item
            var product = new Product { Id = 1, Name = "おにぎり", Price = 160m, TaxRateType = TaxRateType.Reduced8 };
            var items1 = new List<CartItem> { new(product, 1) };
            _mockSaleService.Setup(s => s.Items).Returns(items1);
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary(items1));
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);
            Assert.Equal(1, _viewModel.TotalQuantity);

            // Second event: 3 items (quantity updated)
            var items3 = new List<CartItem> { new(product, 3) };
            _mockSaleService.Setup(s => s.Items).Returns(items3);
            _mockSaleService.Setup(s => s.Summary).Returns(new SaleSummary(items3));
            _mockSaleService.Raise(s => s.CartChanged += null, EventArgs.Empty);

            Assert.Equal(3, _viewModel.TotalQuantity);
            Assert.Equal(480m, _viewModel.SubtotalExcludingTax); // 160 * 3
        }
    }
}
