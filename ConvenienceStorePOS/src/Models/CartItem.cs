using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// カート・売上明細行
    /// </summary>
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public CartItem(Product product, int quantity = 1)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            Quantity = Math.Max(1, quantity);
        }

        public int ProductId => Product.Id;
        public string ProductCode => Product.Code;
        public string ProductName => Product.Name;
        public decimal UnitPrice => Product.Price;
        public TaxRateType TaxRateType => Product.TaxRateType;
        public decimal TaxRate => Product.TaxRate;

        /// <summary>
        /// 税抜小計 (単価 × 数量)
        /// </summary>
        public decimal SubtotalExcludingTax => UnitPrice * Quantity;

        /// <summary>
        /// 明細ごとの消費税額 (切り捨て)
        /// </summary>
        public decimal TaxAmount => Math.Floor(SubtotalExcludingTax * TaxRate);

        /// <summary>
        /// 税込小計
        /// </summary>
        public decimal SubtotalIncludingTax => SubtotalExcludingTax + TaxAmount;
    }
}
