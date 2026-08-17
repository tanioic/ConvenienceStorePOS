namespace ConvenienceStorePOS.Common
{
    /// <summary>
    /// 支払方法区分
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// 現金
        /// </summary>
        Cash = 1,

        /// <summary>
        /// クレジットカード
        /// </summary>
        CreditCard = 2,

        /// <summary>
        /// 電子マネー (交通系IC, iD, QUICPay等)
        /// </summary>
        ElectronicMoney = 3,

        /// <summary>
        /// QR・バーコード決済 (PayPay, 楽天ペイ, d払い等)
        /// </summary>
        QrCode = 4
    }

    public static class PaymentMethodExtensions
    {
        public static string GetDisplayLabel(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "現金",
                PaymentMethod.CreditCard => "クレジットカード",
                PaymentMethod.ElectronicMoney => "電子マネー",
                PaymentMethod.QrCode => "QR・バーコード決済",
                _ => "不明"
            };
        }

        public static string GetIcon(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "💴",
                PaymentMethod.CreditCard => "💳",
                PaymentMethod.ElectronicMoney => "📱",
                PaymentMethod.QrCode => "📲",
                _ => "💰"
            };
        }
    }
}
