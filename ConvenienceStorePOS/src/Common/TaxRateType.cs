namespace ConvenienceStorePOS.Common
{
    /// <summary>
    /// 日本の消費税率区分
    /// </summary>
    public enum TaxRateType
    {
        /// <summary>
        /// 軽減税率 8% (飲食料品、新聞等)
        /// </summary>
        Reduced8 = 8,

        /// <summary>
        /// 標準税率 10% (日用品、酒類、外食等)
        /// </summary>
        Standard10 = 10
    }

    public static class TaxRateExtensions
    {
        /// <summary>
        /// 税率の小数値を取得 (例: 8% -> 0.08m, 10% -> 0.10m)
        /// </summary>
        public static decimal GetRateDecimal(this TaxRateType taxRateType)
        {
            return taxRateType switch
            {
                TaxRateType.Reduced8 => 0.08m,
                TaxRateType.Standard10 => 0.10m,
                _ => 0.10m
            };
        }

        /// <summary>
        /// 表示用ラベルを取得 (例: "軽減8%", "標準10%")
        /// </summary>
        public static string GetDisplayLabel(this TaxRateType taxRateType)
        {
            return taxRateType switch
            {
                TaxRateType.Reduced8 => "軽減8%",
                TaxRateType.Standard10 => "標準10%",
                _ => "10%"
            };
        }
    }
}
