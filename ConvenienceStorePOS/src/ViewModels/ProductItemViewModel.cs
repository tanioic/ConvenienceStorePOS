using CommunityToolkit.Mvvm.ComponentModel;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.ViewModels
{
    public partial class ProductItemViewModel : ObservableObject
    {
        public Product Product { get; }

        public ProductItemViewModel(Product product)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
        }

        public int Id => Product.Id;
        public string Code => Product.Code;
        public string Name => Product.Name;
        public decimal Price => Product.Price;
        public decimal PriceWithTax => Product.PriceWithTax;
        public TaxRateType TaxRateType => Product.TaxRateType;
        public string TaxRateLabel => Product.TaxRateType.GetDisplayLabel();
        public string Category => Product.Category;
        public int StockQuantity => Product.StockQuantity;

        public string FormattedPrice => $"¥{PriceWithTax:N0}";
        public string FormattedPriceTaxExcluded => $"税抜 ¥{Price:N0}";
        public string FormattedPriceDetail => $"¥{Price:N0} (税込 ¥{PriceWithTax:N0})";
    }
}
