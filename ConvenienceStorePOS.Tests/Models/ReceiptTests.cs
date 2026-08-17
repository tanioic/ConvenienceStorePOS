using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using ConvenienceStorePOS.Services;

namespace ConvenienceStorePOS.Tests.Models
{
    public class ReceiptTests
    {
        private static Product CreateProduct(int id, string code, string name, decimal price, TaxRateType taxRateType, string category)
        {
            return new Product
            {
                Id = id,
                Code = code,
                Name = name,
                Price = price,
                TaxRateType = taxRateType,
                Category = category,
                StockQuantity = 100,
                IsActive = true
            };
        }

        private static CartItem CreateCartItem(int id, string code, string name, decimal price, TaxRateType taxRateType, string category, int quantity = 1)
        {
            return new CartItem(CreateProduct(id, code, name, price, taxRateType, category), quantity);
        }

        [Fact]
        public void ReceiptLineItem_FromCartItem_CreatesCorrectLineItem()
        {
            var cartItem = CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 2);

            var lineItem = ReceiptLineItem.FromCartItem(cartItem);

            Assert.Equal("おにぎり 鮭", lineItem.ProductName);
            Assert.Equal(2, lineItem.Quantity);
            Assert.Equal(130, lineItem.UnitPrice);
            Assert.Equal(TaxRateType.Reduced8, lineItem.TaxRateType);
            Assert.Equal(280, lineItem.SubtotalIncludingTax);
        }

        [Fact]
        public void Receipt_Create_SetsAllProperties()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 2),
                CreateCartItem(2, "4902345678901", "紙コップ 160ml", 150, TaxRateType.Standard10, "日用品", 3)
            };
            var summary = new SaleSummary(items);
            var txDate = new DateTime(2026, 8, 17, 14, 30, 0);

            var receipt = Receipt.Create(
                "レジ#01", "鈴木 レジ担当", "TRX-20260817-123", txDate,
                items, summary, PaymentMethod.Cash, 2000, 995);

            Assert.Equal("レジ#01", receipt.RegisterNumber);
            Assert.Equal("鈴木 レジ担当", receipt.StaffName);
            Assert.Equal("TRX-20260817-123", receipt.TransactionNumber);
            Assert.Equal(txDate, receipt.TransactionDateTime);
            Assert.Equal(PaymentMethod.Cash, receipt.PaymentMethod);
            Assert.Equal(2000, receipt.ReceivedAmount);
            Assert.Equal(995, receipt.ChangeAmount);
            Assert.Equal(5, receipt.TotalQuantity);
            Assert.Equal(2, receipt.LineItems.Count);
        }

        [Fact]
        public void Receipt_WithNullLineItems_ReturnsEmptyCollection()
        {
            var summary = SaleSummary.Empty;

            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                Enumerable.Empty<CartItem>(), summary, PaymentMethod.CreditCard, 0, 0);

            Assert.Empty(receipt.LineItems);
            Assert.Equal(0, receipt.TotalQuantity);
            Assert.Equal(0, receipt.TotalAmount);
        }

        [Fact]
        public void Receipt_CalculatesTaxCorrectly_ForReduced8Only()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 3)
            };
            var summary = new SaleSummary(items);

            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 500, 111);

            Assert.Equal(390, receipt.SubtotalExcludingTax);
            Assert.Equal(390, receipt.Reduced8TaxableAmount);
            Assert.Equal(Math.Floor(390 * 0.08m), receipt.Reduced8TaxAmount);
            Assert.Equal(0, receipt.Standard10TaxableAmount);
            Assert.Equal(0, receipt.Standard10TaxAmount);
            Assert.Equal(Math.Floor(390 * 0.08m), receipt.TotalTaxAmount);
            Assert.Equal(390 + Math.Floor(390 * 0.08m), receipt.TotalAmount);
        }

        [Fact]
        public void Receipt_CalculatesTaxCorrectly_ForMixedRates()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 2),
                CreateCartItem(2, "4902345678901", "紙コップ 160ml", 150, TaxRateType.Standard10, "日用品", 1)
            };
            var summary = new SaleSummary(items);

            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 500, 55);

            Assert.Equal(260, receipt.Reduced8TaxableAmount);
            Assert.Equal(150, receipt.Standard10TaxableAmount);
            Assert.Equal(Math.Floor(260 * 0.08m), receipt.Reduced8TaxAmount);
            Assert.Equal(Math.Floor(150 * 0.10m), receipt.Standard10TaxAmount);
            Assert.Equal(410, receipt.SubtotalExcludingTax);
        }
    }

    public class ReceiptServiceTests
    {
        private readonly ReceiptService _service = new();

        private static Product CreateProduct(int id, string code, string name, decimal price, TaxRateType taxRateType, string category)
        {
            return new Product
            {
                Id = id,
                Code = code,
                Name = name,
                Price = price,
                TaxRateType = taxRateType,
                Category = category,
                StockQuantity = 100,
                IsActive = true
            };
        }

        private static CartItem CreateCartItem(int id, string code, string name, decimal price, TaxRateType taxRateType, string category, int quantity = 1)
        {
            return new CartItem(CreateProduct(id, code, name, price, taxRateType, category), quantity);
        }

        [Fact]
        public void GenerateReceiptText_ReturnsNonEmptyString()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 2)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "鈴木 レジ担当", "TRX-20260817-123", new DateTime(2026, 8, 17, 14, 30, 0),
                items, summary, PaymentMethod.Cash, 2000, 1511);

            var text = _service.GenerateReceiptText(receipt);

            Assert.False(string.IsNullOrEmpty(text));
        }

        [Fact]
        public void GenerateReceiptText_ContainsStoreName()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 1000, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("Convenience POS Store", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsTransactionNumber()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-20260817-456", DateTime.Now,
                items, summary, PaymentMethod.Cash, 1000, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("TRX-20260817-456", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsProductNameAndQuantity()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 3)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 500, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("おにぎり 鮭", text);
            Assert.Contains("3", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsTaxLabels()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1),
                CreateCartItem(2, "4902345678901", "紙コップ 160ml", 150, TaxRateType.Standard10, "日用品", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 1000, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("※8%", text);
            Assert.Contains("※10%", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsGrandTotal()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var totalAmount = summary.TotalAmount;
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 1000, 1000 - totalAmount);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("税込合計", text);
            Assert.Contains($"¥{totalAmount:N0}", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsPaymentInfo_ForCash()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 1000, 869);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("[現金]", text);
            Assert.Contains("お預かり", text);
            Assert.Contains("お釣り", text);
            Assert.Contains("¥1,000", text);
            Assert.Contains("¥869", text);
        }

        [Fact]
        public void GenerateReceiptText_Cashless_NoChange()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.CreditCard, summary.TotalAmount, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("[クレジットカード]", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsSeparators()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 500, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("===", text);
            Assert.Contains("---", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsThankYouMessage()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                items, summary, PaymentMethod.Cash, 500, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("ありがとうお越し下さいました", text);
            Assert.Contains("またのご来店をお待ちしております", text);
        }

        [Fact]
        public void GenerateReceiptText_NullReceipt_ReturnsEmpty()
        {
            var text = _service.GenerateReceiptText(null!);

            Assert.Equal(string.Empty, text);
        }

        [Fact]
        public void GenerateReceiptText_EmptyCart_NoItems()
        {
            var summary = SaleSummary.Empty;
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", DateTime.Now,
                Enumerable.Empty<CartItem>(), summary, PaymentMethod.Cash, 0, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("税込合計", text);
            Assert.Contains("¥0", text);
            Assert.DoesNotContain("※8%", text);
            Assert.DoesNotContain("※10%", text);
        }

        [Fact]
        public void GenerateReceiptText_ContainsDateTime()
        {
            var items = new List<CartItem>
            {
                CreateCartItem(1, "4901234567890", "おにぎり 鮭", 130, TaxRateType.Reduced8, "おにぎり・弁当", 1)
            };
            var summary = new SaleSummary(items);
            var txDate = new DateTime(2026, 8, 17, 14, 30, 0);
            var receipt = Receipt.Create(
                "レジ#01", "テスト", "TRX-001", txDate,
                items, summary, PaymentMethod.Cash, 500, 0);

            var text = _service.GenerateReceiptText(receipt);

            Assert.Contains("2026年08月17日 14:30", text);
        }

        [Fact]
        public void CenterText_ReturnsCorrectlyCentered()
        {
            var result = ReceiptService.CenterText("ABC");

            Assert.Equal(32, result.Length);
            Assert.Contains("ABC", result);
            // ABC has 3 chars, padding is 29, left=14, right=15
            Assert.StartsWith(new string(' ', 14), result);
        }

        [Fact]
        public void CenterText_EmptyString_ReturnsSpaces()
        {
            var result = ReceiptService.CenterText("");

            Assert.Equal(32, result.Length);
            Assert.Equal(new string(' ', 32), result);
        }

        [Fact]
        public void FormatYen_ReturnsCorrectFormat()
        {
            Assert.Equal("¥1,000", ReceiptService.FormatYen(1000));
            Assert.Equal("¥0", ReceiptService.FormatYen(0));
            Assert.Equal("¥12,345", ReceiptService.FormatYen(12345));
        }

        [Fact]
        public void TruncateOrPad_TruncatesLongText()
        {
            var result = ReceiptService.TruncateOrPad("Hello World", 5);

            Assert.Equal(5, result.Length);
            Assert.Equal("Hello", result);
        }

        [Fact]
        public void TruncateOrPad_PadsShortText()
        {
            var result = ReceiptService.TruncateOrPad("Hi", 8);

            Assert.Equal(8, result.Length);
            Assert.Equal("Hi      ", result);
        }

        [Fact]
        public void PadLeft_PadsWithSpaces()
        {
            var result = ReceiptService.PadLeft("¥100", 10);

            Assert.Equal(10, result.Length);
            Assert.Equal("      ¥100", result);
        }
    }
}
