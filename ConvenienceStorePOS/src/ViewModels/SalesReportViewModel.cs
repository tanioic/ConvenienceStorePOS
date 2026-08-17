using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Services;

namespace ConvenienceStorePOS.ViewModels
{
    public partial class SalesReportViewModel : ObservableObject
    {
        private readonly IAccountingService _accountingService;

        [ObservableProperty]
        private DateTime _startDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime _endDate = DateTime.Today.AddDays(1);

        [ObservableProperty]
        private decimal _grandTotalAmount;

        [ObservableProperty]
        private decimal _grandTotalTax;

        [ObservableProperty]
        private int _grandTotalTransactions;

        [ObservableProperty]
        private int _grandTotalQuantity;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusError;

        public ObservableCollection<DailySalesSummary> DailySummaries { get; } = new();
        public ObservableCollection<CategorySalesSummary> CategorySummaries { get; } = new();
        public ObservableCollection<PaymentMethodSalesSummary> PaymentSummaries { get; } = new();

        public SalesReportViewModel(IAccountingService accountingService)
        {
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
        }

        public async Task InitializeAsync()
        {
            await LoadReportAsync();
        }

        [RelayCommand]
        public async Task LoadReportAsync()
        {
            try
            {
                if (StartDate >= EndDate)
                {
                    SetStatus("開始日は終了日より前にしてください。", true);
                    return;
                }

                // Daily summaries
                var daily = await _accountingService.GetDailySalesSummaryAsync(StartDate, EndDate);
                DailySummaries.Clear();
                foreach (var d in daily)
                {
                    DailySummaries.Add(d);
                }

                // Category summaries
                var categories = await _accountingService.GetCategorySalesSummaryAsync(StartDate, EndDate);
                CategorySummaries.Clear();
                foreach (var c in categories)
                {
                    CategorySummaries.Add(c);
                }

                // Payment method summaries
                var payments = await _accountingService.GetPaymentMethodSalesSummaryAsync(StartDate, EndDate);
                PaymentSummaries.Clear();
                foreach (var p in payments)
                {
                    PaymentSummaries.Add(p);
                }

                // Calculate grand totals
                GrandTotalAmount = daily.Sum(d => d.TotalAmount);
                GrandTotalTax = daily.Sum(d => d.TotalTax);
                GrandTotalTransactions = daily.Sum(d => d.TransactionCount);
                GrandTotalQuantity = daily.Sum(d => d.TotalQuantity);

                SetStatus($"{StartDate:yyyy/MM/dd} ～ {EndDate:yyyy/MM/dd} の集計を完了しました。", false);
            }
            catch (Exception ex)
            {
                SetStatus($"集計に失敗しました: {ex.Message}", true);
            }
        }

        [RelayCommand]
        public void SetThisWeek()
        {
            StartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            EndDate = DateTime.Today.AddDays(1);
        }

        [RelayCommand]
        public void SetThisMonth()
        {
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today.AddDays(1);
        }

        [RelayCommand]
        public void SetLastMonth()
        {
            var firstOfThisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            StartDate = firstOfThisMonth.AddMonths(-1);
            EndDate = firstOfThisMonth;
        }

        [RelayCommand]
        public void SetToday()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);
        }

        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
        }
    }
}
