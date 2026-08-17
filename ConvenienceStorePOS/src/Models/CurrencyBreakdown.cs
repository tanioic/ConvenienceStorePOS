namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// お釣りの金種内訳
    /// </summary>
    public class CurrencyBreakdown
    {
        public int Bill10000 { get; }
        public int Bill5000 { get; }
        public int Bill1000 { get; }
        public int Coin500 { get; }
        public int Coin100 { get; }
        public int Coin50 { get; }
        public int Coin10 { get; }
        public int Coin5 { get; }
        public int Coin1 { get; }

        public decimal TotalAmount =>
            Bill10000 * 10000m +
            Bill5000 * 5000m +
            Bill1000 * 1000m +
            Coin500 * 500m +
            Coin100 * 100m +
            Coin50 * 50m +
            Coin10 * 10m +
            Coin5 * 5m +
            Coin1 * 1m;

        public CurrencyBreakdown(decimal amount)
        {
            if (amount <= 0) return;

            long remaining = (long)Math.Floor(amount);

            Bill10000 = (int)(remaining / 10000);
            remaining %= 10000;

            Bill5000 = (int)(remaining / 5000);
            remaining %= 5000;

            Bill1000 = (int)(remaining / 1000);
            remaining %= 1000;

            Coin500 = (int)(remaining / 500);
            remaining %= 500;

            Coin100 = (int)(remaining / 100);
            remaining %= 100;

            Coin50 = (int)(remaining / 50);
            remaining %= 50;

            Coin10 = (int)(remaining / 10);
            remaining %= 10;

            Coin5 = (int)(remaining / 5);
            remaining %= 5;

            Coin1 = (int)remaining;
        }

        public string ToFormattedString()
        {
            var parts = new List<string>();
            if (Bill10000 > 0) parts.Add($"1万円札: {Bill10000}枚");
            if (Bill5000 > 0) parts.Add($"5千円札: {Bill5000}枚");
            if (Bill1000 > 0) parts.Add($"千円札: {Bill1000}枚");
            if (Coin500 > 0) parts.Add($"500円玉: {Coin500}枚");
            if (Coin100 > 0) parts.Add($"100円玉: {Coin100}枚");
            if (Coin50 > 0) parts.Add($"50円玉: {Coin50}枚");
            if (Coin10 > 0) parts.Add($"10円玉: {Coin10}枚");
            if (Coin5 > 0) parts.Add($"5円玉: {Coin5}枚");
            if (Coin1 > 0) parts.Add($"1円玉: {Coin1}枚");

            return parts.Count > 0 ? string.Join(" / ", parts) : "なし";
        }
    }
}
