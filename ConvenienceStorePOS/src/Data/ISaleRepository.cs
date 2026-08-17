using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Data
{
    public interface ISaleRepository
    {
        Task<SaleTransaction> SaveSaleAsync(SaleTransaction sale, IEnumerable<SaleDetail> details);
        Task<SaleTransaction?> GetByIdAsync(int id);
        Task<SaleTransaction?> GetByTransactionNumberAsync(string transactionNumber);
        Task<IReadOnlyList<SaleTransaction>> GetRecentSalesAsync(int count = 50);
        Task<IReadOnlyList<SaleTransaction>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<DailySalesSummary>> GetDailySalesSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<CategorySalesSummary>> GetCategorySalesSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IReadOnlyList<PaymentMethodSalesSummary>> GetPaymentMethodSalesSummaryAsync(DateTime startDate, DateTime endDate);
    }

    public class DailySalesSummary
    {
        public DateTime Date { get; set; }
        public int TransactionCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal CashAmount { get; set; }
        public decimal CashlessAmount { get; set; }
    }

    public class CategorySalesSummary
    {
        public string Category { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalAmountExcludingTax { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalAmountIncludingTax { get; set; }
    }

    public class PaymentMethodSalesSummary
    {
        public int PaymentMethod { get; set; }
        public string PaymentMethodLabel { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
