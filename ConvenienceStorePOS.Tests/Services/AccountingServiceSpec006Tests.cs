using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;
using Moq;

namespace ConvenienceStorePOS.Tests.Services
{
    public class AccountingServiceSpec006Tests
    {
        private readonly Mock<ISaleRepository> _mockSaleRepo;
        private readonly AccountingService _service;

        public AccountingServiceSpec006Tests()
        {
            _mockSaleRepo = new Mock<ISaleRepository>();
            _service = new AccountingService(_mockSaleRepo.Object);
        }

        // ========================================================
        // 6.3.1 日別売上集計
        // ========================================================

        [Fact]
        public async Task GetDailySalesSummaryAsync_ReturnsSummaries()
        {
            // Arrange
            var start = new DateTime(2026, 8, 1);
            var end = new DateTime(2026, 8, 17);
            var expected = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = new DateTime(2026, 8, 16), TransactionCount = 5, TotalQuantity = 12, TotalAmount = 5000m, TotalTax = 410m, CashAmount = 3000m, CashlessAmount = 2000m },
                new DailySalesSummary { Date = new DateTime(2026, 8, 15), TransactionCount = 3, TotalQuantity = 8, TotalAmount = 3000m, TotalTax = 245m, CashAmount = 1500m, CashlessAmount = 1500m }
            };
            _mockSaleRepo.Setup(r => r.GetDailySalesSummaryAsync(start, end)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetDailySalesSummaryAsync(start, end);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(8000m, result.Sum(d => d.TotalAmount));
            Assert.Equal(8, result.Sum(d => d.TransactionCount));
            _mockSaleRepo.Verify(r => r.GetDailySalesSummaryAsync(start, end), Times.Once);
        }

        [Fact]
        public async Task GetDailySalesSummaryAsync_EmptyPeriod_ReturnsEmpty()
        {
            // Arrange
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 2);
            _mockSaleRepo.Setup(r => r.GetDailySalesSummaryAsync(start, end))
                         .ReturnsAsync(new List<DailySalesSummary>());

            // Act
            var result = await _service.GetDailySalesSummaryAsync(start, end);

            // Assert
            Assert.Empty(result);
        }

        // ========================================================
        // 6.3.2 商品別売上集計
        // ========================================================

        [Fact]
        public async Task GetCategorySalesSummaryAsync_ReturnsSummaries()
        {
            // Arrange
            var start = new DateTime(2026, 8, 1);
            var end = new DateTime(2026, 8, 17);
            var expected = new List<CategorySalesSummary>
            {
                new CategorySalesSummary { Category = "飲料", TotalQuantity = 20, TotalAmountExcludingTax = 3000m, TotalTax = 240m, TotalAmountIncludingTax = 3240m },
                new CategorySalesSummary { Category = "食品", TotalQuantity = 15, TotalAmountExcludingTax = 5000m, TotalTax = 500m, TotalAmountIncludingTax = 5500m }
            };
            _mockSaleRepo.Setup(r => r.GetCategorySalesSummaryAsync(start, end)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetCategorySalesSummaryAsync(start, end);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("飲料", result[0].Category);
            Assert.Equal(8000m, result.Sum(c => c.TotalAmountExcludingTax));
            _mockSaleRepo.Verify(r => r.GetCategorySalesSummaryAsync(start, end), Times.Once);
        }

        [Fact]
        public async Task GetCategorySalesSummaryAsync_EmptyPeriod_ReturnsEmpty()
        {
            // Arrange
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 2);
            _mockSaleRepo.Setup(r => r.GetCategorySalesSummaryAsync(start, end))
                         .ReturnsAsync(new List<CategorySalesSummary>());

            // Act
            var result = await _service.GetCategorySalesSummaryAsync(start, end);

            // Assert
            Assert.Empty(result);
        }

        // ========================================================
        // 6.3.3 支払方法別売上集計
        // ========================================================

        [Fact]
        public async Task GetPaymentMethodSalesSummaryAsync_ReturnsSummaries()
        {
            // Arrange
            var start = new DateTime(2026, 8, 1);
            var end = new DateTime(2026, 8, 17);
            var expected = new List<PaymentMethodSalesSummary>
            {
                new PaymentMethodSalesSummary { PaymentMethod = 1, PaymentMethodLabel = "現金", TransactionCount = 10, TotalAmount = 15000m },
                new PaymentMethodSalesSummary { PaymentMethod = 2, PaymentMethodLabel = "クレジットカード", TransactionCount = 5, TotalAmount = 8000m }
            };
            _mockSaleRepo.Setup(r => r.GetPaymentMethodSalesSummaryAsync(start, end)).ReturnsAsync(expected);

            // Act
            var result = await _service.GetPaymentMethodSalesSummaryAsync(start, end);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(23000m, result.Sum(p => p.TotalAmount));
            Assert.Equal(15, result.Sum(p => p.TransactionCount));
            _mockSaleRepo.Verify(r => r.GetPaymentMethodSalesSummaryAsync(start, end), Times.Once);
        }

        [Fact]
        public async Task GetPaymentMethodSalesSummaryAsync_EmptyPeriod_ReturnsEmpty()
        {
            // Arrange
            var start = new DateTime(2026, 1, 1);
            var end = new DateTime(2026, 1, 2);
            _mockSaleRepo.Setup(r => r.GetPaymentMethodSalesSummaryAsync(start, end))
                         .ReturnsAsync(new List<PaymentMethodSalesSummary>());

            // Act
            var result = await _service.GetPaymentMethodSalesSummaryAsync(start, end);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPaymentMethodSalesSummaryAsync_DelegatesToRepository()
        {
            // Arrange
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(1);

            // Act
            await _service.GetPaymentMethodSalesSummaryAsync(start, end);

            // Assert
            _mockSaleRepo.Verify(r => r.GetPaymentMethodSalesSummaryAsync(start, end), Times.Once);
        }

        // ========================================================
        // 6.3.4 GetRecentTransactionsAsync
        // ========================================================

        [Fact]
        public async Task GetRecentTransactionsAsync_DelegatesToRepository()
        {
            // Arrange
            _mockSaleRepo.Setup(r => r.GetRecentSalesAsync(10))
                         .ReturnsAsync(new List<SaleTransaction>());

            // Act
            var result = await _service.GetRecentTransactionsAsync(10);

            // Assert
            Assert.Empty(result);
            _mockSaleRepo.Verify(r => r.GetRecentSalesAsync(10), Times.Once);
        }
    }
}
