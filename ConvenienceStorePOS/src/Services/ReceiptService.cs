using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    /// <summary>
    /// レシート生成サービス実装
    /// </summary>
    public class ReceiptService : IReceiptService
    {
        private const int ReceiptWidth = 32;

        public Receipt CreateReceipt(
            string registerNumber,
            string staffName,
            string transactionNumber,
            DateTime transactionDateTime,
            IEnumerable<CartItem> cartItems,
            SaleSummary summary,
            PaymentMethod paymentMethod,
            decimal receivedAmount,
            decimal changeAmount)
        {
            return Receipt.Create(
                registerNumber,
                staffName,
                transactionNumber,
                transactionDateTime,
                cartItems,
                summary,
                paymentMethod,
                receivedAmount,
                changeAmount);
        }

        public string GenerateReceiptText(Receipt receipt)
        {
            if (receipt == null)
                return string.Empty;

            var lines = new List<string>();

            // Header separator
            lines.Add(new string('=', ReceiptWidth));

            // Store name (centered)
            lines.Add(CenterText(receipt.StoreName));

            // Store address (centered)
            if (!string.IsNullOrEmpty(receipt.StoreAddress))
            {
                lines.Add(CenterText(receipt.StoreAddress));
            }

            // Store phone + staff info
            var staffInfo = $"{receipt.StorePhone}  {receipt.RegisterNumber}  {receipt.StaffName}";
            lines.Add(CenterText(staffInfo));

            // Header separator
            lines.Add(new string('=', ReceiptWidth));

            // Transaction number
            lines.Add($"  取引番号: {receipt.TransactionNumber}");

            // Date/time
            var dateTimeStr = receipt.TransactionDateTime.ToString("yyyy年MM月dd日 HH:mm");
            lines.Add($"  {dateTimeStr}");

            // Section separator
            lines.Add(new string('-', ReceiptWidth));

            // Line items
            foreach (var item in receipt.LineItems)
            {
                var taxLabel = item.TaxRateType == TaxRateType.Reduced8 ? "※8%" : "※10%";
                var namePart = TruncateOrPad(item.ProductName, 16);

                // Format: "商品名 数量 ¥小計 ※税率"
                var line = $"  {namePart} {item.Quantity,1}  {FormatYen(item.SubtotalIncludingTax),8} {taxLabel}";
                if (line.Length > ReceiptWidth + 2)
                {
                    // If too long, truncate product name further
                    var shortName = TruncateOrPad(item.ProductName, 12);
                    line = $"  {shortName} {item.Quantity,1}  {FormatYen(item.SubtotalIncludingTax),8} {taxLabel}";
                }
                lines.Add(line);
            }

            // Section separator
            lines.Add(new string('-', ReceiptWidth));

            // Subtotal excluding tax
            lines.Add($"  税抜合計{PadLeft(FormatYen(receipt.SubtotalExcludingTax), ReceiptWidth - 10)}");

            // 8% breakdown
            if (receipt.Reduced8TaxableAmount > 0)
            {
                lines.Add($"  8% 対象: {FormatYen(receipt.Reduced8TaxableAmount)}  消費税: {FormatYen(receipt.Reduced8TaxAmount)}");
            }

            // 10% breakdown
            if (receipt.Standard10TaxableAmount > 0)
            {
                lines.Add($"  10%対象: {FormatYen(receipt.Standard10TaxableAmount)}  消費税: {FormatYen(receipt.Standard10TaxAmount)}");
            }

            // Tax total
            lines.Add($"  消費税合計{PadLeft(FormatYen(receipt.TotalTaxAmount), ReceiptWidth - 12)}");

            // Grand total separator
            lines.Add(new string('=', ReceiptWidth));

            // Grand total
            lines.Add($"  税込合計{PadLeft(FormatYen(receipt.TotalAmount), ReceiptWidth - 10)}");

            // Payment info
            var paymentLabel = $"[{receipt.PaymentMethod.GetDisplayLabel()}]";
            lines.Add($"  {paymentLabel} お預かり{PadLeft(FormatYen(receipt.ReceivedAmount), ReceiptWidth - paymentLabel.Length - 8)}");

            // Change
            lines.Add($"  お釣り{PadLeft(FormatYen(receipt.ChangeAmount), ReceiptWidth - 8)}");

            // Footer separator
            lines.Add(new string('=', ReceiptWidth));

            // Thank you message (centered)
            lines.Add(CenterText("ありがとうお越し下さいました"));
            lines.Add(CenterText("またのご来店をお待ちしております"));

            // Footer separator
            lines.Add(new string('=', ReceiptWidth));

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// テキストを指定幅で中央揃えにする
        /// </summary>
        public static string CenterText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', ReceiptWidth);

            if (text.Length >= ReceiptWidth)
                return text[..ReceiptWidth];

            int padding = ReceiptWidth - text.Length;
            int leftPad = padding / 2;
            int rightPad = padding - leftPad;
            return new string(' ', leftPad) + text + new string(' ', rightPad);
        }

        /// <summary>
        /// 金額を「¥xxx」形式でフォーマット
        /// </summary>
        public static string FormatYen(decimal amount)
        {
            return $"¥{amount:N0}";
        }

        /// <summary>
        /// 文字列を指定幅に切り詰め or パディング
        /// </summary>
        public static string TruncateOrPad(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', maxWidth);

            if (text.Length > maxWidth)
                return text[..maxWidth];

            return text.PadRight(maxWidth);
        }

        /// <summary>
        /// 文字列を右寄せに相当するパディングを追加
        /// </summary>
        public static string PadLeft(string text, int totalWidth)
        {
            if (string.IsNullOrEmpty(text))
                return new string(' ', totalWidth);

            if (text.Length >= totalWidth)
                return text;

            return new string(' ', totalWidth - text.Length) + text;
        }
    }
}
