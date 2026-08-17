using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Data;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;

namespace ConvenienceStorePOS.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly IAccountingService _accountingService;
        private readonly IReceiptService _receiptService;
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly DispatcherTimer? _clockTimer;

        // Store last cart/summary for receipt generation (cart is cleared after payment)
        private IReadOnlyList<CartItem> _lastCartItems = Array.Empty<CartItem>();
        private SaleSummary _lastSaleSummary = SaleSummary.Empty;

        // --- Barcode & Product Search ---
        [ObservableProperty]
        private string _barcodeInput = string.Empty;

        [ObservableProperty]
        private string _searchKeyword = string.Empty;

        [ObservableProperty]
        private string _selectedCategory = "全て";

        [ObservableProperty]
        private CartItemViewModel? _selectedCartItem;

        // --- Cart Summary Properties ---
        [ObservableProperty]
        private int _totalQuantity;

        [ObservableProperty]
        private decimal _subtotalExcludingTax;

        [ObservableProperty]
        private decimal _reduced8TaxableAmount;

        [ObservableProperty]
        private decimal _reduced8TaxAmount;

        [ObservableProperty]
        private decimal _standard10TaxableAmount;

        [ObservableProperty]
        private decimal _standard10TaxAmount;

        [ObservableProperty]
        private decimal _totalTaxAmount;

        [ObservableProperty]
        private decimal _totalAmount;

        // --- Status & Store Info ---
        [ObservableProperty]
        private string _statusMessage = "準備完了";

        [ObservableProperty]
        private bool _isStatusError;

        [ObservableProperty]
        private string _currentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

        [ObservableProperty]
        private string _staffName = "鈴木 レジ担当";

        [ObservableProperty]
        private string _registerNumber = "レジ #01";

        // --- SPEC-003 Accounting Properties ---
        [ObservableProperty]
        private bool _isAccountingModalOpen;

        [ObservableProperty]
        private bool _isTransactionCompletedModalOpen;

        [ObservableProperty]
        private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

        [ObservableProperty]
        private string _receivedAmountInput = string.Empty;

        [ObservableProperty]
        private decimal _receivedAmount;

        [ObservableProperty]
        private decimal _changeAmount;

        [ObservableProperty]
        private decimal _shortageAmount;

        [ObservableProperty]
        private bool _isPaymentAllowed;

        [ObservableProperty]
        private string _changeBreakdownText = "なし";

        [ObservableProperty]
        private string _paymentErrorMessage = string.Empty;

        [ObservableProperty]
        private SaleTransaction? _completedTransaction;

        // --- SPEC-004 Receipt Properties ---
        [ObservableProperty]
        private bool _isReceiptModalOpen;

        [ObservableProperty]
        private string _receiptText = string.Empty;

        [ObservableProperty]
        private Receipt? _currentReceipt;

        // --- Collections ---
        public ObservableCollection<CartItemViewModel> CartItems { get; } = new();
        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<ProductItemViewModel> DisplayProducts { get; } = new();

        public bool HasCartItems => CartItems.Count > 0;
        public bool CanOpenAccounting => HasCartItems && TotalAmount > 0;

        public MainViewModel(
            ISaleService saleService,
            IProductService productService,
            IAccountingService accountingService,
            IReceiptService receiptService,
            IDatabaseInitializer databaseInitializer)
        {
            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));

            _saleService.CartChanged += OnCartChanged;

            // Clock timer for POS header
            try
            {
                if (Dispatcher.CurrentDispatcher != null)
                {
                    _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    _clockTimer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    _clockTimer.Start();
                }
            }
            catch
            {
                // Dispatcher might not be available in test environments
            }
        }

        public async Task InitializeAsync()
        {
            await _databaseInitializer.InitializeAsync();
            await LoadCategoriesAsync();
            await LoadProductsAsync();
            UpdateCartUI();
        }

        public async Task LoadCategoriesAsync()
        {
            Categories.Clear();
            Categories.Add("全て");

            var categories = await _productService.GetCategoriesAsync();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }

            if (string.IsNullOrEmpty(SelectedCategory) || !Categories.Contains(SelectedCategory))
            {
                SelectedCategory = "全て";
            }
        }

        public async Task LoadProductsAsync()
        {
            var products = await _productService.SearchProductsAsync(SearchKeyword, SelectedCategory);
            DisplayProducts.Clear();
            foreach (var product in products)
            {
                DisplayProducts.Add(new ProductItemViewModel(product));
            }
        }

        [RelayCommand]
        public async Task RegisterByBarcodeAsync()
        {
            var code = BarcodeInput?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            var item = await _saleService.AddProductByCodeAsync(code);
            if (item != null)
            {
                SetStatus($"【登録】{item.ProductName} (¥{item.UnitPrice:N0}) を追加しました", isError: false);
            }
            else
            {
                SetStatus($"【エラー】商品コード「{code}」が見つかりませんでした", isError: true);
            }

            BarcodeInput = string.Empty;
        }

        [RelayCommand]
        public void SelectProduct(ProductItemViewModel? item)
        {
            if (item == null) return;

            var cartItem = _saleService.AddProduct(item.Product);
            SetStatus($"【登録】{cartItem.ProductName} を追加しました", isError: false);
        }

        [RelayCommand]
        public void IncreaseQuantity(int productId)
        {
            _saleService.IncrementQuantity(productId, 1);
        }

        [RelayCommand]
        public void DecreaseQuantity(int productId)
        {
            _saleService.DecrementQuantity(productId, 1);
        }

        [RelayCommand]
        public void RemoveItem(int productId)
        {
            _saleService.RemoveItem(productId);
            SetStatus("明細を取消しました", isError: false);
        }

        [RelayCommand]
        public void ClearCart()
        {
            if (CartItems.Count == 0) return;
            _saleService.ClearCart();
            SetStatus("取引を全取消しました", isError: false);
        }

        [RelayCommand]
        public async Task SelectCategoryAsync(string? category)
        {
            if (category == null) return;
            SelectedCategory = category;
            await LoadProductsAsync();
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

        // ========================================================
        // SPEC-003: Accounting Commands & Methods
        // ========================================================

        [RelayCommand]
        public void OpenAccounting()
        {
            if (!CanOpenAccounting)
            {
                SetStatus("カートに商品がありません。", isError: true);
                return;
            }

            SelectedPaymentMethod = PaymentMethod.Cash;
            ReceivedAmountInput = string.Empty;
            PaymentErrorMessage = string.Empty;
            UpdateAccountingCalculations();
            IsAccountingModalOpen = true;
            SetStatus("会計画面を開きました。支払方法を選択してください。", isError: false);
        }

        [RelayCommand]
        public void CloseAccounting()
        {
            if (IsReceiptModalOpen)
            {
                CloseReceipt();
                return;
            }
            IsAccountingModalOpen = false;
            PaymentErrorMessage = string.Empty;
            SetStatus("会計を中断し、明細画面に戻りました。", isError: false);
        }

        [RelayCommand]
        public void SelectPaymentMethod(PaymentMethod method)
        {
            SelectedPaymentMethod = method;
            PaymentErrorMessage = string.Empty;
            UpdateAccountingCalculations();
        }

        [RelayCommand]
        public void InputKeypad(string? key)
        {
            if (string.IsNullOrEmpty(key)) return;

            if (key == "C")
            {
                ReceivedAmountInput = string.Empty;
            }
            else if (key == "BS")
            {
                if (ReceivedAmountInput.Length > 0)
                {
                    ReceivedAmountInput = ReceivedAmountInput[..^1];
                }
            }
            else if (key == "00")
            {
                if (!string.IsNullOrEmpty(ReceivedAmountInput) && ReceivedAmountInput != "0")
                {
                    ReceivedAmountInput += "00";
                }
            }
            else
            {
                if (ReceivedAmountInput == "0")
                {
                    ReceivedAmountInput = key;
                }
                else
                {
                    ReceivedAmountInput += key;
                }
            }

            UpdateAccountingCalculations();
        }

        [RelayCommand]
        public void QuickCash(string? action)
        {
            if (string.IsNullOrEmpty(action)) return;

            if (action == "Exact")
            {
                ReceivedAmountInput = ((long)TotalAmount).ToString();
            }
            else if (action.StartsWith("+"))
            {
                if (long.TryParse(action[1..], out var addAmount))
                {
                    long current = long.TryParse(ReceivedAmountInput, out var curVal) ? curVal : 0;
                    ReceivedAmountInput = (current + addAmount).ToString();
                }
            }
            else if (long.TryParse(action, out var fixedAmount))
            {
                ReceivedAmountInput = fixedAmount.ToString();
            }

            UpdateAccountingCalculations();
        }

        partial void OnReceivedAmountInputChanged(string value)
        {
            UpdateAccountingCalculations();
        }

        private void UpdateAccountingCalculations()
        {
            if (SelectedPaymentMethod == PaymentMethod.Cash)
            {
                if (decimal.TryParse(ReceivedAmountInput, out var val))
                {
                    ReceivedAmount = val;
                }
                else
                {
                    ReceivedAmount = 0m;
                }

                if (ReceivedAmount >= TotalAmount)
                {
                    ChangeAmount = ReceivedAmount - TotalAmount;
                    ShortageAmount = 0m;
                    IsPaymentAllowed = true;
                    var breakdown = _accountingService.CalculateCurrencyBreakdown(ChangeAmount);
                    ChangeBreakdownText = breakdown.ToFormattedString();
                }
                else
                {
                    ChangeAmount = 0m;
                    ShortageAmount = TotalAmount - ReceivedAmount;
                    IsPaymentAllowed = false;
                    ChangeBreakdownText = "なし";
                }
            }
            else
            {
                // Cashless (Credit card, e-money, QR)
                ReceivedAmount = TotalAmount;
                ChangeAmount = 0m;
                ShortageAmount = 0m;
                IsPaymentAllowed = TotalAmount > 0;
                ChangeBreakdownText = "なし";
            }
        }

        [RelayCommand]
        public async Task ConfirmPaymentAsync()
        {
            if (!IsPaymentAllowed)
            {
                PaymentErrorMessage = "お預かり金額が不足しています。";
                return;
            }

            var result = await _accountingService.ProcessPaymentAsync(
                SelectedPaymentMethod,
                ReceivedAmount,
                _saleService.Items,
                _saleService.Summary,
                StaffName,
                RegisterNumber);

            if (!result.IsSuccess || result.Transaction == null)
            {
                PaymentErrorMessage = result.ErrorMessage ?? "決済処理に失敗しました。";
                return;
            }

            CompletedTransaction = result.Transaction;
            IsAccountingModalOpen = false;
            IsTransactionCompletedModalOpen = true;

            // Save cart data for receipt generation before clearing
            _lastCartItems = _saleService.Items;
            _lastSaleSummary = _saleService.Summary;

            // Clear active cart in sale service
            _saleService.ClearCart();

            SetStatus($"【会計完了】取引番号: {CompletedTransaction.TransactionNumber} - お釣り: ¥{CompletedTransaction.ChangeAmount:N0}", isError: false);
        }

        [RelayCommand]
        public void FinishTransaction()
        {
            IsTransactionCompletedModalOpen = false;
            CompletedTransaction = null;
            CurrentReceipt = null;
            ReceiptText = string.Empty;
            ReceivedAmountInput = string.Empty;
            UpdateAccountingCalculations();
            SetStatus("取引を完了しました。次の商品をスキャンしてください。", isError: false);
        }

        // ========================================================
        // SPEC-004: Receipt Commands & Methods
        // ========================================================

        [RelayCommand]
        public void ShowReceipt()
        {
            if (CompletedTransaction == null)
            {
                SetStatus("レシートを表示する取引がありません。", isError: true);
                return;
            }

            var cartItems = _saleService.Items;
            var summary = _saleService.Summary;

            CurrentReceipt = _receiptService.CreateReceipt(
                CompletedTransaction.RegisterNumber,
                CompletedTransaction.StaffName,
                CompletedTransaction.TransactionNumber,
                CompletedTransaction.CreatedAt,
                cartItems.Count > 0 ? cartItems : _lastCartItems,
                summary.TotalQuantity > 0 ? summary : _lastSaleSummary,
                CompletedTransaction.PaymentMethod,
                CompletedTransaction.ReceivedAmount,
                CompletedTransaction.ChangeAmount);

            ReceiptText = _receiptService.GenerateReceiptText(CurrentReceipt);
            IsReceiptModalOpen = true;
        }

        [RelayCommand]
        public void CloseReceipt()
        {
            IsReceiptModalOpen = false;
        }

        [RelayCommand]
        public void PrintReceipt()
        {
            if (string.IsNullOrEmpty(ReceiptText))
            {
                SetStatus("印刷するレシートがありません。", isError: true);
                return;
            }

            try
            {
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var flowDocument = new System.Windows.Documents.FlowDocument(
                        new System.Windows.Documents.Paragraph(
                            new System.Windows.Documents.Run(ReceiptText)))
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Consolas, Courier New"),
                        FontSize = 10,
                        PagePadding = new System.Windows.Thickness(20)
                    };

                    var documentPaginator = ((System.Windows.Documents.IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    printDialog.PrintDocument(documentPaginator, $"レシート - {CompletedTransaction?.TransactionNumber ?? ""}");

                    SetStatus("レシートを印刷しました。", isError: false);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"印刷に失敗しました: {ex.Message}", isError: true);
            }
        }

        private void OnCartChanged(object? sender, EventArgs e)
        {
            UpdateCartUI();
        }

        private void UpdateCartUI()
        {
            var currentItems = _saleService.Items;

            CartItems.Clear();
            foreach (var item in currentItems)
            {
                CartItems.Add(new CartItemViewModel(
                    item,
                    onIncrease: id => IncreaseQuantity(id),
                    onDecrease: id => DecreaseQuantity(id),
                    onRemove: id => RemoveItem(id)
                ));
            }

            var summary = _saleService.Summary;
            TotalQuantity = summary.TotalQuantity;
            SubtotalExcludingTax = summary.SubtotalExcludingTax;
            Reduced8TaxableAmount = summary.Reduced8TaxableAmount;
            Reduced8TaxAmount = summary.Reduced8TaxAmount;
            Standard10TaxableAmount = summary.Standard10TaxableAmount;
            Standard10TaxAmount = summary.Standard10TaxAmount;
            TotalTaxAmount = summary.TotalTaxAmount;
            TotalAmount = summary.TotalAmount;

            OnPropertyChanged(nameof(HasCartItems));
            OnPropertyChanged(nameof(CanOpenAccounting));

            UpdateAccountingCalculations();
        }

        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            IsStatusError = isError;
        }
    }
}
