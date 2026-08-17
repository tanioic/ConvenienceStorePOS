using ConvenienceStorePOS.Common;

namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// レシート明細行
    /// </summary>
    public class ReceiptLineItem
    {
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }
        public TaxRateType TaxRateType { get; }
        public decimal SubtotalIncludingTax { get; }

        public ReceiptLineItem(string productName, int quantity, decimal unitPrice, TaxRateType taxRateType, decimal subtotalIncludingTax)
        {
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxRateType = taxRateType;
            SubtotalIncludingTax = subtotalIncludingTax;
        }

        public static ReceiptLineItem FromCartItem(CartItem item)
        {
            return new ReceiptLineItem(
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.TaxRateType,
                item.SubtotalIncludingTax);
        }
    }

    /// <summary>
    /// レシートデータ（不変オブジェクト）
    /// </summary>
    public class Receipt
    {
        // 店舗情報
        public string StoreName { get; }
        public string StoreAddress { get; }
        public string StorePhone { get; }
        public string RegisterNumber { get; }
        public string StaffName { get; }

        // 取引情報
        public string TransactionNumber { get; }
        public DateTime TransactionDateTime { get; }

        // 明細情報
        public IReadOnlyList<ReceiptLineItem> LineItems { get; }

        // 金額集計
        public int TotalQuantity { get; }
        public decimal SubtotalExcludingTax { get; }
        public decimal Reduced8TaxableAmount { get; }
        public decimal Reduced8TaxAmount { get; }
        public decimal Standard10TaxableAmount { get; }
        public decimal Standard10TaxAmount { get; }
        public decimal TotalTaxAmount { get; }
        public decimal TotalAmount { get; }

        // 支払情報
        public PaymentMethod PaymentMethod { get; }
        public decimal ReceivedAmount { get; }
        public decimal ChangeAmount { get; }

        public Receipt(
            string storeName,
            string storeAddress,
            string storePhone,
            string registerNumber,
            string staffName,
            string transactionNumber,
            DateTime transactionDateTime,
            IEnumerable<ReceiptLineItem> lineItems,
            SaleSummary summary,
            PaymentMethod paymentMethod,
            decimal receivedAmount,
            decimal changeAmount)
        {
            StoreName = storeName;
            StoreAddress = storeAddress;
            StorePhone = storePhone;
            RegisterNumber = registerNumber;
            StaffName = staffName;
            TransactionNumber = transactionNumber;
            TransactionDateTime = transactionDateTime;
            LineItems = lineItems?.ToList().AsReadOnly() ?? (IReadOnlyList<ReceiptLineItem>)Array.Empty<ReceiptLineItem>();
            TotalQuantity = summary?.TotalQuantity ?? 0;
            SubtotalExcludingTax = summary?.SubtotalExcludingTax ?? 0m;
            Reduced8TaxableAmount = summary?.Reduced8TaxableAmount ?? 0m;
            Reduced8TaxAmount = summary?.Reduced8TaxAmount ?? 0m;
            Standard10TaxableAmount = summary?.Standard10TaxableAmount ?? 0m;
            Standard10TaxAmount = summary?.Standard10TaxAmount ?? 0m;
            TotalTaxAmount = summary?.TotalTaxAmount ?? 0m;
            TotalAmount = summary?.TotalAmount ?? 0m;
            PaymentMethod = paymentMethod;
            ReceivedAmount = receivedAmount;
            ChangeAmount = changeAmount;
        }

        public static Receipt Create(
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
            var lineItems = cartItems.Select(ReceiptLineItem.FromCartItem);
            return new Receipt(
                storeName: "Convenience POS Store",
                storeAddress: "東京都渋谷区〇〇1-2-3",
                storePhone: "03-1234-5678",
                registerNumber: registerNumber,
                staffName: staffName,
                transactionNumber: transactionNumber,
                transactionDateTime: transactionDateTime,
                lineItems: lineItems,
                summary: summary,
                paymentMethod: paymentMethod,
                receivedAmount: receivedAmount,
                changeAmount: changeAmount);
        }
    }
}
