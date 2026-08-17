using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Xunit;

namespace ConvenienceStorePOS.Tests.Models
{
    public class CartItemTests
    {
        [Fact]
        public void CartItem_SubtotalAndTax_CalculatesCorrectly()
        {
            // Arrange: 8% tax product, unit price 160, quantity 3
            var product = new Product
            {
                Id = 1,
                Code = "4901001000018",
                Name = "手巻おにぎり 熟成紅しゃけ",
                Price = 160m,
                TaxRateType = TaxRateType.Reduced8,
                Category = "おにぎり・弁当"
            };

            var cartItem = new CartItem(product, 3);

            // Act & Assert
            Assert.Equal(3, cartItem.Quantity);
            // 160 * 3 = 480
            Assert.Equal(480m, cartItem.SubtotalExcludingTax);
            // 480 * 0.08 = 38.4 -> Floor -> 38
            Assert.Equal(38m, cartItem.TaxAmount);
            // 480 + 38 = 518
            Assert.Equal(518m, cartItem.SubtotalIncludingTax);
        }

        [Fact]
        public void CartItem_Standard10Percent_SubtotalCalculatesCorrectly()
        {
            // Arrange: 10% tax product, unit price 650, quantity 2
            var product = new Product
            {
                Id = 2,
                Code = "4901005000016",
                Name = "ビニール傘",
                Price = 650m,
                TaxRateType = TaxRateType.Standard10,
                Category = "日用品"
            };

            var cartItem = new CartItem(product, 2);

            // Act & Assert
            // 650 * 2 = 1300
            Assert.Equal(1300m, cartItem.SubtotalExcludingTax);
            // 1300 * 0.10 = 130
            Assert.Equal(130m, cartItem.TaxAmount);
            // 1300 + 130 = 1430
            Assert.Equal(1430m, cartItem.SubtotalIncludingTax);
        }

        [Fact]
        public void CartItem_ThrowsOnNullProduct()
        {
            Assert.Throws<ArgumentNullException>(() => new CartItem(null!, 1));
        }
    }
}
