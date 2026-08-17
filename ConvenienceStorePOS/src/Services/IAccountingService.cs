using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public interface IAccountingService
    {
        CurrencyBreakdown CalculateCurrencyBreakdown(decimal changeAmount);
        Task<PaymentResult> ProcessPaymentAsync(
            PaymentMethod paymentMethod,
            decimal receivedAmount,
            IEnumerable<CartItem> cartItems,
            SaleSummary summary,
            string staffName,
            string registerNumber);
        Task<IReadOnlyList<SaleTransaction>> GetRecentTransactionsAsync(int count = 50);
        Task<IReadOnlyList<DailySalesSummary>> GetDailySalesSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<CategorySalesSummary>> GetCategorySalesSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<PaymentMethodSalesSummary>> GetPaymentMethodSalesSummaryAsync(DateTime startDate, DateTime endDate);
    }
}
