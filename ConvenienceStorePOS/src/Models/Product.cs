using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// 商品マスタエンティティ
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        /// <summary>
        /// JANコード / 商品コード (一意)
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 商品名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 単価 (税抜価格)
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 消費税区分 (8% / 10%)
        /// </summary>
        public TaxRateType TaxRateType { get; set; } = TaxRateType.Reduced8;

        /// <summary>
        /// カテゴリ名 (例: おにぎり・弁当, 飲料, ホットスナック, 菓子・デザート, 日用品)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 在庫数
        /// </summary>
        public int StockQuantity { get; set; } = 100;

        /// <summary>
        /// 有効フラグ
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 税率の小数値 (例: 0.08m)
        /// </summary>
        public decimal TaxRate => TaxRateType.GetRateDecimal();

        /// <summary>
        /// 税込単価 (端数切り捨て)
        /// </summary>
        public decimal PriceWithTax => Math.Floor(Price * (1m + TaxRate));
    }
}
