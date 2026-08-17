using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// 売上取引明細データ
    /// </summary>
    public class SaleDetail
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public TaxRateType TaxRateType { get; set; } = TaxRateType.Reduced8;
        public decimal TaxRate => TaxRateType.GetRateDecimal();
        public int Quantity { get; set; }
        public decimal SubtotalExcludingTax => UnitPrice * Quantity;
        public decimal TaxAmount => Math.Floor(SubtotalExcludingTax * TaxRate);
        public decimal SubtotalIncludingTax => SubtotalExcludingTax + TaxAmount;

        public static SaleDetail FromCartItem(CartItem item, int saleId = 0)
        {
            return new SaleDetail
            {
                SaleId = saleId,
                ProductId = item.ProductId,
                ProductCode = item.ProductCode,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                TaxRateType = item.TaxRateType,
                Quantity = item.Quantity
            };
        }
    }
}
