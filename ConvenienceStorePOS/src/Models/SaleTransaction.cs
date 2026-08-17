using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// 売上取引データ（ヘッダ）
    /// </summary>
    public class SaleTransaction
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int TotalQuantity { get; set; }
        public decimal SubtotalExcludingTax { get; set; }
        public decimal Reduced8TaxableAmount { get; set; }
        public decimal Reduced8TaxAmount { get; set; }
        public decimal Standard10TaxableAmount { get; set; }
        public decimal Standard10TaxAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal ReceivedAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        public string StaffName { get; set; } = string.Empty;
        public string RegisterNumber { get; set; } = string.Empty;

        public List<SaleDetail> Details { get; set; } = new();

        public static string GenerateTransactionNumber()
        {
            return $"TRX-{DateTime.Now:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        }
    }
}
