using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Data
{
    public interface ISaleRepository
    {
        Task<SaleTransaction> SaveSaleAsync(SaleTransaction sale, IEnumerable<SaleDetail> details);
        Task<SaleTransaction?> GetByIdAsync(int id);
        Task<SaleTransaction?> GetByTransactionNumberAsync(string transactionNumber);
        Task<IReadOnlyList<SaleTransaction>> GetRecentSalesAsync(int count = 50);
    }
}
