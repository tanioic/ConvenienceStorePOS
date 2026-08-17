using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Xunit;

namespace ConvenienceStorePOS.Tests.Models
{
    public class SaleSummaryTests
    {
        [Fact]
        public void SaleSummary_Empty_ReturnsZeros()
        {
            var summary = new SaleSummary();

            Assert.Equal(0, summary.TotalQuantity);
            Assert.Equal(0m, summary.SubtotalExcludingTax);
            Assert.Equal(0m, summary.Reduced8TaxableAmount);
            Assert.Equal(0m, summary.Reduced8TaxAmount);
            Assert.Equal(0m, summary.Standard10TaxableAmount);
            Assert.Equal(0m, summary.Standard10TaxAmount);
            Assert.Equal(0m, summary.TotalTaxAmount);
            Assert.Equal(0m, summary.TotalAmount);
        }

        [Fact]
        public void SaleSummary_MixedTaxes_CalculatesAccurately()
        {
            // Arrange
            // 1. Food item (8%): Onigiri ¥160 x 2 = ¥320
            var onigiri = new Product
            {
                Id = 1,
                Code = "4901001000018",
                Name = "熟成紅しゃけ",
                Price = 160m,
                TaxRateType = TaxRateType.Reduced8,
                Category = "おにぎり・弁当"
            };

            // 2. Beverage item (8%): Green tea ¥130 x 1 = ¥130
            var tea = new Product
            {
                Id = 2,
                Code = "4901002000015",
                Name = "緑茶 500ml",
                Price = 130m,
                TaxRateType = TaxRateType.Reduced8,
                Category = "飲料"
            };

            // 3. Daily goods item (10%): Umbrella ¥650 x 1 = ¥650
            var umbrella = new Product
            {
                Id = 3,
                Code = "4901005000016",
                Name = "ビニール傘",
                Price = 650m,
                TaxRateType = TaxRateType.Standard10,
                Category = "日用品"
            };

            var items = new List<CartItem>
            {
                new(onigiri, 2),
                new(tea, 1),
                new(umbrella, 1)
            };

            // Act
            var summary = new SaleSummary(items);

            // Assert
            // Total items: 2 + 1 + 1 = 4
            Assert.Equal(4, summary.TotalQuantity);

            // Subtotal excl. tax: 320 + 130 + 650 = 1100
            Assert.Equal(1100m, summary.SubtotalExcludingTax);

            // 8% Taxable: 320 + 130 = 450
            Assert.Equal(450m, summary.Reduced8TaxableAmount);
            // 8% Tax: 450 * 0.08 = 36
            Assert.Equal(36m, summary.Reduced8TaxAmount);

            // 10% Taxable: 650
            Assert.Equal(650m, summary.Standard10TaxableAmount);
            // 10% Tax: 650 * 0.10 = 65
            Assert.Equal(65m, summary.Standard10TaxAmount);

            // Total Tax: 36 + 65 = 101
            Assert.Equal(101m, summary.TotalTaxAmount);

            // Grand Total: 1100 + 101 = 1201
            Assert.Equal(1201m, summary.TotalAmount);
        }
    }
}
