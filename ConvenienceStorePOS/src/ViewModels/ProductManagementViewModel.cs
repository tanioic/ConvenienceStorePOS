using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;

namespace ConvenienceStorePOS.ViewModels
{
    public partial class ProductManagementViewModel : ObservableObject
    {
        private readonly IProductService _productService;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "全て";

        [ObservableProperty]
        private ProductItemViewModel? _selectedProduct;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private string _editCode = string.Empty;

        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private decimal _editPrice;

        [ObservableProperty]
        private TaxRateType _editTaxRateType = TaxRateType.Reduced8;

        [ObservableProperty]
        private string _editCategory = string.Empty;

        [ObservableProperty]
        private int _editStockQuantity = 100;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isStatusError;

        public ObservableCollection<ProductItemViewModel> Products { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<string> EditCategories { get; } = new();

        public TaxRateType[] TaxRateTypes { get; } = Enum.GetValues<TaxRateType>();

        public bool HasSelectedProduct => SelectedProduct != null;

        public ProductManagementViewModel(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadProductsAsync();
        }

        public async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            Categories.Add("全て");

            EditCategories.Clear();

            var categories = await _productService.GetCategoriesAsync();
            foreach (var category in categories)
            {
                Categories.Add(category);
                EditCategories.Add(category);
            }

            if (string.IsNullOrEmpty(SelectedCategory) || !Categories.Contains(SelectedCategory))
            {
                SelectedCategory = "全て";
            }
        }

        public async Task LoadProductsAsync()
        {
            var products = await _productService.SearchProductsAsync(SearchKeyword, SelectedCategory);
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(new ProductItemViewModel(product));
            }
        }

        [RelayCommand]
        public async Task SearchAsync()
        {
            await LoadProductsAsync();
        }

        [RelayCommand]
        public async Task ClearSearchAsync()
        {
            SearchKeyword = string.Empty;
            await LoadProductsAsync();
        }

        [RelayCommand]
        public async Task SelectCategoryAsync(string? category)
        {
            if (category == null) return;
            SelectedCategory = category;
            await LoadProductsAsync();
        }

        [RelayCommand]
        public void StartAddNew()
        {
            IsEditing = true;
            SelectedProduct = null;
            EditCode = string.Empty;
            EditName = string.Empty;
            EditPrice = 0;
            EditTaxRateType = TaxRateType.Reduced8;
            EditCategory = EditCategories.Count > 0 ? EditCategories[0] : string.Empty;
            EditStockQuantity = 100;
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        public void StartEdit(ProductItemViewModel? item)
        {
            if (item == null) return;

            IsEditing = true;
            SelectedProduct = item;
            EditCode = item.Code;
            EditName = item.Name;
            EditPrice = item.Price;
            EditTaxRateType = item.TaxRateType;
            EditCategory = item.Category;
            EditStockQuantity = item.StockQuantity;
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        public void CancelEdit()
        {
            IsEditing = false;
            SelectedProduct = null;
            StatusMessage = string.Empty;
        }

        [RelayCommand]
        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(EditCode))
            {
                SetStatus("商品コードを入力してください。", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(EditName))
            {
                SetStatus("商品名を入力してください。", true);
                return;
            }
            if (EditPrice < 0)
            {
                SetStatus("単価は0以上にしてください。", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(EditCategory))
            {
                SetStatus("カテゴリを選択してください。", true);
                return;
            }

            try
            {
                if (SelectedProduct != null)
                {
                    var product = new Product
                    {
                        Id = SelectedProduct.Id,
                        Code = EditCode.Trim(),
                        Name = EditName.Trim(),
                        Price = EditPrice,
                        TaxRateType = EditTaxRateType,
                        Category = EditCategory,
                        StockQuantity = EditStockQuantity,
                        IsActive = true
                    };
                    await _productService.UpdateProductAsync(product);
                    SetStatus($"「{EditName}」を更新しました。", false);
                }
                else
                {
                    var product = new Product
                    {
                        Code = EditCode.Trim(),
                        Name = EditName.Trim(),
                        Price = EditPrice,
                        TaxRateType = EditTaxRateType,
                        Category = EditCategory,
                        StockQuantity = EditStockQuantity,
                        IsActive = true
                    };
                    await _productService.AddProductAsync(product);
                    SetStatus($"「{EditName}」を新規登録しました。", false);
                }

                IsEditing = false;
                SelectedProduct = null;
                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"保存に失敗しました: {ex.Message}", true);
            }
        }

        [RelayCommand]
        public async Task DeleteProductAsync(ProductItemViewModel? item)
        {
            if (item == null) return;

            try
            {
                await _productService.DeleteProductAsync(item.Id);
                SetStatus($"「{item.Name}」を削除しました。", false);

                if (SelectedProduct?.Id == item.Id)
                {
                    IsEditing = false;
                    SelectedProduct = null;
                }

                await LoadProductsAsync();
            }
            catch (Exception ex)
            {
                SetStatus($"削除に失敗しました: {ex.Message}", true);
            }
        }

        partial void OnSelectedProductChanged(ProductItemViewModel? value)
        {
            OnPropertyChanged(nameof(HasSelectedProduct));
        }

        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
        }
    }
}
