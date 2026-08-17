using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Xunit;

namespace ConvenienceStorePOS.Tests.Models
{
    /// <summary>
    /// SPEC-002: 売上集計・明細確認
    /// SaleSummary の税計算（インボイス制度対応・税率区分別Floor切り捨て）を検証するテスト
    /// </summary>
    public class SaleSummarySpec002Tests
    {
        // --- Helper: テスト用商品生成 ---
        private static Product MakeProduct(int id, decimal price, TaxRateType taxType, string category = "テスト")
            => new() { Id = id, Code = $"CODE{id:D3}", Name = $"商品{id}", Price = price, TaxRateType = taxType, Category = category };

        // =====================================================
        // 2.1.1 合計点数 (TotalQuantity)
        // =====================================================

        [Fact]
        public void TotalQuantity_SingleItem_ReturnsQuantity()
        {
            var product = MakeProduct(1, 100m, TaxRateType.Reduced8);
            var items = new List<CartItem> { new(product, 3) };

            var summary = new SaleSummary(items);

            Assert.Equal(3, summary.TotalQuantity);
        }

        [Fact]
        public void TotalQuantity_MultipleItems_ReturnsSumOfAllQuantities()
        {
            var p1 = MakeProduct(1, 100m, TaxRateType.Reduced8);
            var p2 = MakeProduct(2, 200m, TaxRateType.Standard10);
            var p3 = MakeProduct(3, 300m, TaxRateType.Reduced8);
            var items = new List<CartItem> { new(p1, 2), new(p2, 5), new(p3, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(8, summary.TotalQuantity); // 2 + 5 + 1
        }

        // =====================================================
        // 2.1.2 税抜合計金額 (SubtotalExcludingTax)
        // =====================================================

        [Fact]
        public void SubtotalExcludingTax_CalculatesCorrectly()
        {
            // Arrange: 100 x 2 + 250 x 3 = 200 + 750 = 950
            var p1 = MakeProduct(1, 100m, TaxRateType.Reduced8);
            var p2 = MakeProduct(2, 250m, TaxRateType.Standard10);
            var items = new List<CartItem> { new(p1, 2), new(p2, 3) };

            var summary = new SaleSummary(items);

            Assert.Equal(950m, summary.SubtotalExcludingTax);
        }

        // =====================================================
        // 2.1.3 軽減税率8%のみ (Reduced8 only)
        // =====================================================

        [Fact]
        public void Reduced8TaxAmount_OnlyReducedItems_CalculatesWithFloor()
        {
            // ¥333 x 1 = 333; 333 * 0.08 = 26.64 -> Floor = 26
            var product = MakeProduct(1, 333m, TaxRateType.Reduced8);
            var items = new List<CartItem> { new(product, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(333m, summary.Reduced8TaxableAmount);
            Assert.Equal(26m, summary.Reduced8TaxAmount);  // Floor(26.64) = 26
            Assert.Equal(0m, summary.Standard10TaxableAmount);
            Assert.Equal(0m, summary.Standard10TaxAmount);
            Assert.Equal(359m, summary.TotalAmount);       // 333 + 26
        }

        [Fact]
        public void Reduced8TaxAmount_MultipleReducedItems_AggregatesBeforeFloor()
        {
            // ¥125 x 3 = 375; ¥88 x 1 = 88; Total taxable = 463; 463 * 0.08 = 37.04 -> Floor = 37
            var p1 = MakeProduct(1, 125m, TaxRateType.Reduced8);
            var p2 = MakeProduct(2, 88m, TaxRateType.Reduced8);
            var items = new List<CartItem> { new(p1, 3), new(p2, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(463m, summary.Reduced8TaxableAmount);
            Assert.Equal(37m, summary.Reduced8TaxAmount);   // Floor(37.04) = 37
        }

        // =====================================================
        // 2.1.4 標準税率10%のみ (Standard10 only)
        // =====================================================

        [Fact]
        public void Standard10TaxAmount_OnlyStandardItems_CalculatesWithFloor()
        {
            // ¥777 x 1 = 777; 777 * 0.10 = 77.7 -> Floor = 77
            var product = MakeProduct(1, 777m, TaxRateType.Standard10);
            var items = new List<CartItem> { new(product, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(0m, summary.Reduced8TaxableAmount);
            Assert.Equal(0m, summary.Reduced8TaxAmount);
            Assert.Equal(777m, summary.Standard10TaxableAmount);
            Assert.Equal(77m, summary.Standard10TaxAmount);  // Floor(77.7) = 77
            Assert.Equal(854m, summary.TotalAmount);          // 777 + 77
        }

        // =====================================================
        // 2.1.5 混在税率 (Mixed Taxes) — SPEC-002 核心シナリオ
        // =====================================================

        [Fact]
        public void SaleSummary_MixedTaxRates_CalculatesAllFieldsCorrectly()
        {
            // Onigiri (8%): ¥160 x 2 = ¥320
            // Tea (8%):     ¥130 x 1 = ¥130
            // Umbrella (10%): ¥650 x 1 = ¥650
            var onigiri  = MakeProduct(1, 160m, TaxRateType.Reduced8, "おにぎり・弁当");
            var tea      = MakeProduct(2, 130m, TaxRateType.Reduced8, "飲料");
            var umbrella = MakeProduct(3, 650m, TaxRateType.Standard10, "日用品");

            var items = new List<CartItem>
            {
                new(onigiri, 2),
                new(tea, 1),
                new(umbrella, 1)
            };

            var summary = new SaleSummary(items);

            // TotalQuantity: 2 + 1 + 1 = 4
            Assert.Equal(4, summary.TotalQuantity);

            // SubtotalExcludingTax: 320 + 130 + 650 = 1100
            Assert.Equal(1100m, summary.SubtotalExcludingTax);

            // 8% bracket: 320 + 130 = 450; tax = Floor(450 * 0.08) = Floor(36) = 36
            Assert.Equal(450m, summary.Reduced8TaxableAmount);
            Assert.Equal(36m, summary.Reduced8TaxAmount);

            // 10% bracket: 650; tax = Floor(650 * 0.10) = Floor(65) = 65
            Assert.Equal(650m, summary.Standard10TaxableAmount);
            Assert.Equal(65m, summary.Standard10TaxAmount);

            // TotalTaxAmount: 36 + 65 = 101
            Assert.Equal(101m, summary.TotalTaxAmount);

            // TotalAmount: 1100 + 101 = 1201
            Assert.Equal(1201m, summary.TotalAmount);
        }

        // =====================================================
        // 端数切り捨て確認 (Floor rounding)
        // =====================================================

        [Fact]
        public void SaleSummary_FractionalTax_IsFloorRounded()
        {
            // ¥1 x 1 (8%): 1 * 0.08 = 0.08 -> Floor = 0
            var product = MakeProduct(1, 1m, TaxRateType.Reduced8);
            var items = new List<CartItem> { new(product, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(0m, summary.Reduced8TaxAmount);
            Assert.Equal(1m, summary.TotalAmount); // 1 + 0
        }

        [Fact]
        public void SaleSummary_FractionalTax10_IsFloorRounded()
        {
            // ¥3 x 1 (10%): 3 * 0.10 = 0.3 -> Floor = 0
            var product = MakeProduct(1, 3m, TaxRateType.Standard10);
            var items = new List<CartItem> { new(product, 1) };

            var summary = new SaleSummary(items);

            Assert.Equal(0m, summary.Standard10TaxAmount);
            Assert.Equal(3m, summary.TotalAmount); // 3 + 0
        }

        // =====================================================
        // 空カート (Empty Cart)
        // =====================================================

        [Fact]
        public void SaleSummary_EmptyList_ReturnsAllZeros()
        {
            var summary = new SaleSummary(new List<CartItem>());

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
        public void SaleSummary_DefaultConstructor_ReturnsAllZeros()
        {
            var summary = new SaleSummary();

            Assert.Equal(0, summary.TotalQuantity);
            Assert.Equal(0m, summary.TotalAmount);
        }

        [Fact]
        public void SaleSummary_NullItems_ReturnsAllZeros()
        {
            var summary = new SaleSummary(null);

            Assert.Equal(0, summary.TotalQuantity);
            Assert.Equal(0m, summary.TotalAmount);
        }

        // =====================================================
        // SaleSummary.Empty 静的プロパティ
        // =====================================================

        [Fact]
        public void SaleSummary_Empty_StaticProperty_ReturnsZeros()
        {
            var empty = SaleSummary.Empty;

            Assert.Equal(0, empty.TotalQuantity);
            Assert.Equal(0m, empty.TotalAmount);
        }

        // =====================================================
        // 2.2 CartItem 明細行の税計算
        // =====================================================

        [Fact]
        public void CartItem_Reduced8_TaxAmountCalculatedWithFloor()
        {
            // ¥333 x 2 = 666; 666 * 0.08 = 53.28 -> Floor = 53
            var product = MakeProduct(1, 333m, TaxRateType.Reduced8);
            var item = new CartItem(product, 2);

            Assert.Equal(666m, item.SubtotalExcludingTax);
            Assert.Equal(53m, item.TaxAmount);
            Assert.Equal(719m, item.SubtotalIncludingTax);
        }

        [Fact]
        public void CartItem_Standard10_TaxAmountCalculatedWithFloor()
        {
            // ¥99 x 3 = 297; 297 * 0.10 = 29.7 -> Floor = 29
            var product = MakeProduct(1, 99m, TaxRateType.Standard10);
            var item = new CartItem(product, 3);

            Assert.Equal(297m, item.SubtotalExcludingTax);
            Assert.Equal(29m, item.TaxAmount);
            Assert.Equal(326m, item.SubtotalIncludingTax);
        }

        [Fact]
        public void CartItem_DefaultQuantity_IsOne()
        {
            var product = MakeProduct(1, 200m, TaxRateType.Reduced8);
            var item = new CartItem(product);

            Assert.Equal(1, item.Quantity);
        }

        [Fact]
        public void CartItem_ExposesProductProperties_Correctly()
        {
            var product = MakeProduct(42, 500m, TaxRateType.Standard10, "日用品");
            var item = new CartItem(product, 1);

            Assert.Equal(42, item.ProductId);
            Assert.Equal("CODE042", item.ProductCode);
            Assert.Equal("商品42", item.ProductName);
            Assert.Equal(500m, item.UnitPrice);
            Assert.Equal(TaxRateType.Standard10, item.TaxRateType);
        }
    }
}
