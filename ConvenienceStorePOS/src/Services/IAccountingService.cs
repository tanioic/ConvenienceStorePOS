using ConvenienceStorePOS.Common;
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
    }
}
