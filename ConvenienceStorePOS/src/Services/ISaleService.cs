using ConvenienceStorePOS.Models;

namespace ConvenienceStorePOS.Services
{
    public interface ISaleService
    {
        event EventHandler? CartChanged;

        IReadOnlyList<CartItem> Items { get; }
        SaleSummary Summary { get; }

        Task<CartItem?> AddProductByCodeAsync(string code, int quantity = 1);
        CartItem AddProduct(Product product, int quantity = 1);
        bool UpdateQuantity(int productId, int newQuantity);
        bool IncrementQuantity(int productId, int amount = 1);
        bool DecrementQuantity(int productId, int amount = 1);
        bool RemoveItem(int productId);
        void ClearCart();
    }
}
