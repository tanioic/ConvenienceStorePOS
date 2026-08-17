namespace ConvenienceStorePOS.Models
{
    /// <summary>
    /// 会計・決済結果
    /// </summary>
    public class PaymentResult
    {
        public bool IsSuccess { get; }
        public SaleTransaction? Transaction { get; }
        public decimal ChangeAmount { get; }
        public CurrencyBreakdown? ChangeBreakdown { get; }
        public string? ErrorMessage { get; }

        private PaymentResult(bool isSuccess, SaleTransaction? transaction, decimal changeAmount, CurrencyBreakdown? changeBreakdown, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Transaction = transaction;
            ChangeAmount = changeAmount;
            ChangeBreakdown = changeBreakdown;
            ErrorMessage = errorMessage;
        }

        public static PaymentResult Success(SaleTransaction transaction, decimal changeAmount)
        {
            return new PaymentResult(true, transaction, changeAmount, new CurrencyBreakdown(changeAmount), null);
        }

        public static PaymentResult Failed(string errorMessage)
        {
            return new PaymentResult(false, null, 0m, null, errorMessage);
        }
    }
}
