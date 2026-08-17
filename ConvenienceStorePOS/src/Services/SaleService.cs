using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public class SaleService : ISaleService
    {
        private readonly IProductService _productService;
        private readonly List<CartItem> _items = new();
        private readonly object _lock = new();

        public event EventHandler? CartChanged;

        public IReadOnlyList<CartItem> Items
        {
            get
            {
                lock (_lock)
                {
                    return _items.ToList().AsReadOnly();
                }
            }
        }

        public SaleSummary Summary
        {
            get
            {
                lock (_lock)
                {
                    return new SaleSummary(_items);
                }
            }
        }

        public SaleService(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        public async Task<CartItem?> AddProductByCodeAsync(string code, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var product = await _productService.FindByCodeAsync(code.Trim());
            if (product == null)
            {
                return null;
            }

            return AddProduct(product, quantity);
        }

        public CartItem AddProduct(Product product, int quantity = 1)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (quantity <= 0) quantity = 1;

            CartItem cartItem;
            lock (_lock)
            {
                var existingItem = _items.FirstOrDefault(x => x.ProductId == product.Id);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    cartItem = existingItem;
                }
                else
                {
                    cartItem = new CartItem(product, quantity);
                    _items.Add(cartItem);
                }
            }

            OnCartChanged();
            return cartItem;
        }

        public bool UpdateQuantity(int productId, int newQuantity)
        {
            bool changed = false;
            lock (_lock)
            {
                var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
                if (existingItem != null)
                {
                    if (newQuantity <= 0)
                    {
                        _items.Remove(existingItem);
                    }
                    else
                    {
                        existingItem.Quantity = newQuantity;
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                OnCartChanged();
            }
            return changed;
        }

        public bool IncrementQuantity(int productId, int amount = 1)
        {
            if (amount <= 0) amount = 1;

            bool changed = false;
            lock (_lock)
            {
                var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity += amount;
                    changed = true;
                }
            }

            if (changed)
            {
                OnCartChanged();
            }
            return changed;
        }

        public bool DecrementQuantity(int productId, int amount = 1)
        {
            if (amount <= 0) amount = 1;

            bool changed = false;
            lock (_lock)
            {
                var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
                if (existingItem != null)
                {
                    if (existingItem.Quantity <= amount)
                    {
                        _items.Remove(existingItem);
                    }
                    else
                    {
                        existingItem.Quantity -= amount;
                    }
                    changed = true;
                }
            }

            if (changed)
            {
                OnCartChanged();
            }
            return changed;
        }

        public bool RemoveItem(int productId)
        {
            bool removed;
            lock (_lock)
            {
                var item = _items.FirstOrDefault(x => x.ProductId == productId);
                removed = item != null && _items.Remove(item);
            }

            if (removed)
            {
                OnCartChanged();
            }
            return removed;
        }

        public void ClearCart()
        {
            lock (_lock)
            {
                _items.Clear();
            }
            OnCartChanged();
        }

        protected virtual void OnCartChanged()
        {
            CartChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
