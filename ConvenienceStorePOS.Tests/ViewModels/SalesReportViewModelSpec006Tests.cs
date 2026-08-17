using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Services;
using ConvenienceStorePOS.ViewModels;
using Moq;

namespace ConvenienceStorePOS.Tests.ViewModels
{
    public class SalesReportViewModelSpec006Tests
    {
        private readonly Mock<IAccountingService> _mockAccountingService;
        private readonly SalesReportViewModel _viewModel;

        public SalesReportViewModelSpec006Tests()
        {
            _mockAccountingService = new Mock<IAccountingService>();
            _viewModel = new SalesReportViewModel(_mockAccountingService.Object);
        }

        // ========================================================
        // 6.4.1 InitializeAsync
        // ========================================================

        [Fact]
        public async Task InitializeAsync_LoadsAllSummaries()
        {
            // Arrange
            var daily = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = DateTime.Today, TransactionCount = 5, TotalQuantity = 10, TotalAmount = 5000m, TotalTax = 410m, CashAmount = 3000m, CashlessAmount = 2000m }
            };
            var categories = new List<CategorySalesSummary>
            {
                new CategorySalesSummary { Category = "飲料", TotalQuantity = 10, TotalAmountExcludingTax = 3000m, TotalTax = 240m, TotalAmountIncludingTax = 3240m }
            };
            var payments = new List<PaymentMethodSalesSummary>
            {
                new PaymentMethodSalesSummary { PaymentMethod = 1, PaymentMethodLabel = "現金", TransactionCount = 3, TotalAmount = 3000m }
            };

            _mockAccountingService.Setup(s => s.GetDailySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(daily);
            _mockAccountingService.Setup(s => s.GetCategorySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(categories);
            _mockAccountingService.Setup(s => s.GetPaymentMethodSalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(payments);

            // Act
            await _viewModel.InitializeAsync();

            // Assert
            Assert.Single(_viewModel.DailySummaries);
            Assert.Single(_viewModel.CategorySummaries);
            Assert.Single(_viewModel.PaymentSummaries);
            Assert.Equal(5000m, _viewModel.GrandTotalAmount);
            Assert.Equal(410m, _viewModel.GrandTotalTax);
            Assert.Equal(5, _viewModel.GrandTotalTransactions);
            Assert.Equal(10, _viewModel.GrandTotalQuantity);
        }

        // ========================================================
        // 6.4.2 Date Range Presets
        // ========================================================

        [Fact]
        public void SetTodayCommand_SetsTodayRange()
        {
            // Act
            _viewModel.SetTodayCommand.Execute(null);

            // Assert
            Assert.Equal(DateTime.Today, _viewModel.StartDate);
            Assert.Equal(DateTime.Today.AddDays(1), _viewModel.EndDate);
        }

        [Fact]
        public void SetThisWeekCommand_SetsWeekRange()
        {
            // Act
            _viewModel.SetThisWeekCommand.Execute(null);

            // Assert - .NET DayOfWeek: Sunday=0, Monday=1, ...
            // SetThisWeek uses -(int)DayOfWeek, so start is Sunday of current week
            Assert.Equal(DayOfWeek.Sunday, _viewModel.StartDate.DayOfWeek);
            Assert.Equal(DateTime.Today.AddDays(1), _viewModel.EndDate);
        }

        [Fact]
        public void SetThisMonthCommand_SetsMonthRange()
        {
            // Act
            _viewModel.SetThisMonthCommand.Execute(null);

            // Assert
            Assert.Equal(1, _viewModel.StartDate.Day);
            Assert.Equal(DateTime.Today.Month, _viewModel.StartDate.Month);
        }

        [Fact]
        public void SetLastMonthCommand_SetsLastMonthRange()
        {
            // Act
            _viewModel.SetLastMonthCommand.Execute(null);

            // Assert
            var expectedStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            Assert.Equal(expectedStart, _viewModel.StartDate);
        }

        // ========================================================
        // 6.4.3 Validation
        // ========================================================

        [Fact]
        public async Task LoadReportAsync_InvalidDateRange_SetsErrorStatus()
        {
            // Arrange - start >= end
            _viewModel.StartDate = new DateTime(2026, 8, 17);
            _viewModel.EndDate = new DateTime(2026, 8, 1);

            // Act
            await _viewModel.LoadReportCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.IsStatusError);
            Assert.Contains("開始日", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task LoadReportAsync_CalculatesGrandTotals()
        {
            // Arrange
            var daily = new List<DailySalesSummary>
            {
                new DailySalesSummary { Date = DateTime.Today, TransactionCount = 3, TotalQuantity = 5, TotalAmount = 3000m, TotalTax = 245m, CashAmount = 2000m, CashlessAmount = 1000m },
                new DailySalesSummary { Date = DateTime.Today.AddDays(-1), TransactionCount = 2, TotalQuantity = 3, TotalAmount = 2000m, TotalTax = 163m, CashAmount = 1000m, CashlessAmount = 1000m }
            };

            _mockAccountingService.Setup(s => s.GetDailySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(daily);
            _mockAccountingService.Setup(s => s.GetCategorySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<CategorySalesSummary>());
            _mockAccountingService.Setup(s => s.GetPaymentMethodSalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<PaymentMethodSalesSummary>());

            // Act
            await _viewModel.LoadReportCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(5000m, _viewModel.GrandTotalAmount);
            Assert.Equal(408m, _viewModel.GrandTotalTax);
            Assert.Equal(5, _viewModel.GrandTotalTransactions);
            Assert.Equal(8, _viewModel.GrandTotalQuantity);
        }

        [Fact]
        public async Task LoadReportAsync_EmptyData_SetsZeroTotals()
        {
            // Arrange
            _mockAccountingService.Setup(s => s.GetDailySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<DailySalesSummary>());
            _mockAccountingService.Setup(s => s.GetCategorySalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<CategorySalesSummary>());
            _mockAccountingService.Setup(s => s.GetPaymentMethodSalesSummaryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<PaymentMethodSalesSummary>());

            // Act
            await _viewModel.LoadReportCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(0m, _viewModel.GrandTotalAmount);
            Assert.Equal(0m, _viewModel.GrandTotalTax);
            Assert.Equal(0, _viewModel.GrandTotalTransactions);
            Assert.Equal(0, _viewModel.GrandTotalQuantity);
        }

        [Fact]
        public void Constructor_NullService_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SalesReportViewModel(null!));
        }
    }
}
