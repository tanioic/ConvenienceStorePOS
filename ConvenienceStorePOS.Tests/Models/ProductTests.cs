using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Xunit;

namespace ConvenienceStorePOS.Tests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Product_PriceWithTax_CalculatesCorrectly_ForReduced8Percent()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Code = "4901001000018",
                Name = "手巻おにぎり 熟成紅しゃけ",
                Price = 160m,
                TaxRateType = TaxRateType.Reduced8,
                Category = "おにぎり・弁当"
            };

            // Act & Assert
            Assert.Equal(0.08m, product.TaxRate);
            Assert.Equal("軽減8%", product.TaxRateType.GetDisplayLabel());
            // 160 * 1.08 = 172.8 -> Floor -> 172
            Assert.Equal(172m, product.PriceWithTax);
        }

        [Fact]
        public void Product_PriceWithTax_CalculatesCorrectly_ForStandard10Percent()
        {
            // Arrange
            var product = new Product
            {
                Id = 2,
                Code = "4901005000016",
                Name = "ビニール傘",
                Price = 650m,
                TaxRateType = TaxRateType.Standard10,
                Category = "日用品"
            };

            // Act & Assert
            Assert.Equal(0.10m, product.TaxRate);
            Assert.Equal("標準10%", product.TaxRateType.GetDisplayLabel());
            // 650 * 1.10 = 715
            Assert.Equal(715m, product.PriceWithTax);
        }
    }
}
