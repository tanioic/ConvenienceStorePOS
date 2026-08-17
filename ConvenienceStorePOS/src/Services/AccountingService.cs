using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly ISaleRepository _saleRepository;

        public AccountingService(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository ?? throw new ArgumentNullException(nameof(saleRepository));
        }

        public CurrencyBreakdown CalculateCurrencyBreakdown(decimal changeAmount)
        {
            return new CurrencyBreakdown(changeAmount);
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            PaymentMethod paymentMethod,
            decimal receivedAmount,
            IEnumerable<CartItem> cartItems,
            SaleSummary summary,
            string staffName,
            string registerNumber)
        {
            var itemsList = cartItems?.ToList() ?? new List<CartItem>();
            if (itemsList.Count == 0 || summary.TotalQuantity == 0)
            {
                return PaymentResult.Failed("カートに商品がありません。");
            }

            decimal finalReceived;
            decimal changeAmount;

            if (paymentMethod == PaymentMethod.Cash)
            {
                if (receivedAmount < summary.TotalAmount)
                {
                    var shortage = summary.TotalAmount - receivedAmount;
                    return PaymentResult.Failed($"お預かり金額が不足しています（不足額: ¥{shortage:N0}）");
                }

                finalReceived = receivedAmount;
                changeAmount = receivedAmount - summary.TotalAmount;
            }
            else
            {
                // Cashless payment methods: amount received is exact total
                finalReceived = summary.TotalAmount;
                changeAmount = 0m;
            }

            var transaction = new SaleTransaction
            {
                TransactionNumber = SaleTransaction.GenerateTransactionNumber(),
                CreatedAt = DateTime.Now,
                TotalQuantity = summary.TotalQuantity,
                SubtotalExcludingTax = summary.SubtotalExcludingTax,
                Reduced8TaxableAmount = summary.Reduced8TaxableAmount,
                Reduced8TaxAmount = summary.Reduced8TaxAmount,
                Standard10TaxableAmount = summary.Standard10TaxableAmount,
                Standard10TaxAmount = summary.Standard10TaxAmount,
                TotalTaxAmount = summary.TotalTaxAmount,
                TotalAmount = summary.TotalAmount,
                PaymentMethod = paymentMethod,
                ReceivedAmount = finalReceived,
                ChangeAmount = changeAmount,
                StaffName = staffName,
                RegisterNumber = registerNumber
            };

            var details = itemsList.Select(x => SaleDetail.FromCartItem(x)).ToList();

            var savedSale = await _saleRepository.SaveSaleAsync(transaction, details);

            return PaymentResult.Success(savedSale, changeAmount);
        }

        public async Task<IReadOnlyList<SaleTransaction>> GetRecentTransactionsAsync(int count = 50)
        {
            return await _saleRepository.GetRecentSalesAsync(count);
        }
    }
}
