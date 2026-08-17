using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    /// <summary>
    /// レシート生成サービス
    /// </summary>
    public interface IReceiptService
    {
        /// <summary>
        /// カート情報と決済結果からReceiptオブジェクトを生成する
        /// </summary>
        Receipt CreateReceipt(
            string registerNumber,
            string staffName,
            string transactionNumber,
            DateTime transactionDateTime,
            IEnumerable<CartItem> cartItems,
            SaleSummary summary,
            Common.PaymentMethod paymentMethod,
            decimal receivedAmount,
            decimal changeAmount);

        /// <summary>
        /// Receiptオブジェクトからテキスト形式のレシートを生成する
        /// </summary>
        string GenerateReceiptText(Receipt receipt);
    }
}
