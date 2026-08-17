using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// 売上サマリー（合計金額および消費税計算）
    /// 日本のインボイス制度に準拠した税率別端数計算（税率区分ごとの対象合計金額に対して税率を乗じ端数切り捨て）
    /// </summary>
    public class SaleSummary
    {
        /// <summary>
        /// 合計点数
        /// </summary>
        public int TotalQuantity { get; }

        /// <summary>
        /// 税抜合計金額
        /// </summary>
        public decimal SubtotalExcludingTax { get; }

        /// <summary>
        /// 8% (軽減税率) 対象税抜金額
        /// </summary>
        public decimal Reduced8TaxableAmount { get; }

        /// <summary>
        /// 8% 消費税額 (切り捨て)
        /// </summary>
        public decimal Reduced8TaxAmount { get; }

        /// <summary>
        /// 10% (標準税率) 対象税抜金額
        /// </summary>
        public decimal Standard10TaxableAmount { get; }

        /// <summary>
        /// 10% 消費税額 (切り捨て)
        /// </summary>
        public decimal Standard10TaxAmount { get; }

        /// <summary>
        /// 合計消費税額 (8%税額 + 10%税額)
        /// </summary>
        public decimal TotalTaxAmount => Reduced8TaxAmount + Standard10TaxAmount;

        /// <summary>
        /// 税込合計金額 (税抜合計 + 合計消費税額)
        /// </summary>
        public decimal TotalAmount => SubtotalExcludingTax + TotalTaxAmount;

        public SaleSummary(IEnumerable<CartItem>? items = null)
        {
            if (items == null)
            {
                TotalQuantity = 0;
                SubtotalExcludingTax = 0m;
                Reduced8TaxableAmount = 0m;
                Reduced8TaxAmount = 0m;
                Standard10TaxableAmount = 0m;
                Standard10TaxAmount = 0m;
                return;
            }

            var itemList = items.ToList();
            TotalQuantity = itemList.Sum(x => x.Quantity);
            SubtotalExcludingTax = itemList.Sum(x => x.SubtotalExcludingTax);

            Reduced8TaxableAmount = itemList
                .Where(x => x.TaxRateType == TaxRateType.Reduced8)
                .Sum(x => x.SubtotalExcludingTax);
            Reduced8TaxAmount = Math.Floor(Reduced8TaxableAmount * 0.08m);

            Standard10TaxableAmount = itemList
                .Where(x => x.TaxRateType == TaxRateType.Standard10)
                .Sum(x => x.SubtotalExcludingTax);
            Standard10TaxAmount = Math.Floor(Standard10TaxableAmount * 0.10m);
        }

        public static SaleSummary Empty => new SaleSummary();
    }
}
