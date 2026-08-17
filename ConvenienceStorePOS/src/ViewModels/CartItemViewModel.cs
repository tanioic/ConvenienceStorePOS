using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.ViewModels
{
    public partial class CartItemViewModel : ObservableObject
    {
        private readonly CartItem _cartItem;
        private readonly Action<int>? _onIncrease;
        private readonly Action<int>? _onDecrease;
        private readonly Action<int>? _onRemove;

        public CartItem CartItem => _cartItem;

        public CartItemViewModel(
            CartItem cartItem,
            Action<int>? onIncrease = null,
            Action<int>? onDecrease = null,
            Action<int>? onRemove = null)
        {
            _cartItem = cartItem ?? throw new ArgumentNullException(nameof(cartItem));
            _onIncrease = onIncrease;
            _onDecrease = onDecrease;
            _onRemove = onRemove;
        }

        public int ProductId => _cartItem.ProductId;
        public string ProductCode => _cartItem.ProductCode;
        public string ProductName => _cartItem.ProductName;
        public decimal UnitPrice => _cartItem.UnitPrice;
        public int Quantity => _cartItem.Quantity;
        public TaxRateType TaxRateType => _cartItem.TaxRateType;
        public string TaxRateLabel => _cartItem.TaxRateType.GetDisplayLabel();
        public decimal SubtotalExcludingTax => _cartItem.SubtotalExcludingTax;
        public decimal TaxAmount => _cartItem.TaxAmount;
        public decimal SubtotalIncludingTax => _cartItem.SubtotalIncludingTax;

        public string FormattedUnitPrice => $"¥{UnitPrice:N0}";
        public string FormattedSubtotal => $"¥{SubtotalIncludingTax:N0}";
        public string FormattedSubtotalExclTax => $"¥{SubtotalExcludingTax:N0}";
        public string FormattedUnitWithTax => $"¥{Math.Floor(UnitPrice * (1m + _cartItem.TaxRate)):N0}";

        [RelayCommand]
        private void IncreaseQuantity()
        {
            _onIncrease?.Invoke(ProductId);
        }

        [RelayCommand]
        private void DecreaseQuantity()
        {
            _onDecrease?.Invoke(ProductId);
        }

        [RelayCommand]
        private void Remove()
        {
            _onRemove?.Invoke(ProductId);
        }
    }
}
