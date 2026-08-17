# 03_DomainModel.md - ドメインモデル

## モデル一覧

### Product (`src/Models/Product.cs`)
商品マスタエンティティ。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Id` | `int` | 商品ID（主キー） |
| `Code` | `string` | JANコード / 商品コード（一意） |
| `Name` | `string` | 商品名 |
| `Price` | `decimal` | 単価（税抜価格） |
| `TaxRateType` | `TaxRateType` | 消費税区分（Reduced8 / Standard10） |
| `Category` | `string` | カテゴリ名 |
| `StockQuantity` | `int` | 在庫数（デフォルト: 100） |
| `IsActive` | `bool` | 有効フラグ（デフォルト: true） |

算出プロパティ:
- `TaxRate` → `TaxRateType.GetRateDecimal()` (0.08m or 0.10m)
- `PriceWithTax` → `Math.Floor(Price * (1m + TaxRate))`

### CartItem (`src/Models/CartItem.cs`)
カート・売上明細行。`Product` と数量を保持。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Product` | `Product` | 参照商品 |
| `Quantity` | `int` | 数量（最低1） |

委譲プロパティ: `ProductId`, `ProductCode`, `ProductName`, `UnitPrice`, `TaxRateType`, `TaxRate`

算出プロパティ:
- `SubtotalExcludingTax` → `UnitPrice * Quantity`
- `TaxAmount` → `Math.Floor(SubtotalExcludingTax * TaxRate)`
- `SubtotalIncludingTax` → `SubtotalExcludingTax + TaxAmount`

コンストラクタで `product` が null の場合は `ArgumentNullException`、`quantity` が1未満の場合は1にクランプ。

### SaleSummary (`src/Models/SaleSummary.cs`)
売上サマリー（不変オブジェクト）。税率別端数切り捨て方式で税額を計算。

| プロパティ | 型 | 説明 |
|---|---|---|
| `TotalQuantity` | `int` | 合計点数 |
| `SubtotalExcludingTax` | `decimal` | 税抜合計金額 |
| `Reduced8TaxableAmount` | `decimal` | 8%対象税抜金額 |
| `Reduced8TaxAmount` | `decimal` | 8%消費税額（Floor） |
| `Standard10TaxableAmount` | `decimal` | 10%対象税抜金額 |
| `Standard10TaxAmount` | `decimal` | 10%消費税額（Floor） |
| `TotalTaxAmount` | `decimal` | 消費税合計（8% + 10%） |
| `TotalAmount` | `decimal` | 税込合計（税抜合計 + 消費税合計） |

静的プロパティ: `SaleSummary.Empty` (全値が0)

### SaleTransaction (`src/Models/SaleTransaction.cs`)
売上取引データ（ヘッダ）。DB永続化対象。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Id` | `int` | データベース主キー |
| `TransactionNumber` | `string` | 取引番号（例: `TRX-20260817123456-123`） |
| `CreatedAt` | `DateTime` | 取引日時 |
| `TotalQuantity` | `int` | 合計点数 |
| `SubtotalExcludingTax` | `decimal` | 税抜合計 |
| `Reduced8TaxableAmount` | `decimal` | 8%対象税抜額 |
| `Reduced8TaxAmount` | `decimal` | 8%消費税額 |
| `Standard10TaxableAmount` | `decimal` | 10%対象税抜額 |
| `Standard10TaxAmount` | `decimal` | 10%消費税額 |
| `TotalTaxAmount` | `decimal` | 消費税合計 |
| `TotalAmount` | `decimal` | 税込合計金額 |
| `PaymentMethod` | `PaymentMethod` | 支払方法区分 |
| `ReceivedAmount` | `decimal` | お預かり金額 |
| `ChangeAmount` | `decimal` | お釣り金額 |
| `StaffName` | `string` | レジ担当者名 |
| `RegisterNumber` | `string` | レジ番号 |
| `Details` | `List<SaleDetail>` | 取引明細リスト |

静的メソッド: `GenerateTransactionNumber()` → `TRX-{yyyyMMddHHmmss}-{random 100-999}`

### SaleDetail (`src/Models/SaleDetail.cs`)
売上取引明細データ。DB永続化対象。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Id` | `int` | データベース主キー |
| `SaleId` | `int` | 取引ヘッダID（外部キー） |
| `ProductId` | `int` | 商品ID |
| `ProductCode` | `string` | JANコード |
| `ProductName` | `string` | 商品名 |
| `UnitPrice` | `decimal` | 単価（税抜） |
| `TaxRateType` | `TaxRateType` | 税率区分 |
| `Quantity` | `int` | 数量 |

静的ファクトリ: `FromCartItem(CartItem item, int saleId = 0)`

### Receipt (`src/Models/Receipt.cs`)
レシートデータ（不変オブジェクト）。`ReceiptLineItem` を内包。

| プロパティ | 型 | 説明 |
|---|---|---|
| `StoreName` | `string` | 店舗名 |
| `StoreAddress` | `string` | 店舗住所 |
| `StorePhone` | `string` | 電話番号 |
| `RegisterNumber` | `string` | レジ番号 |
| `StaffName` | `string` | 担当者名 |
| `TransactionNumber` | `string` | 取引番号 |
| `TransactionDateTime` | `DateTime` | 取引日時 |
| `LineItems` | `IReadOnlyList<ReceiptLineItem>` | 明細行リスト |
| `TotalQuantity` | `int` | 合計点数 |
| `SubtotalExcludingTax` | `decimal` | 税抜合計 |
| `Reduced8TaxableAmount` | `decimal` | 8%対象税抜額 |
| `Reduced8TaxAmount` | `decimal` | 8%消費税額 |
| `Standard10TaxableAmount` | `decimal` | 10%対象税抜額 |
| `Standard10TaxAmount` | `decimal` | 10%消費税額 |
| `TotalTaxAmount` | `decimal` | 消費税合計 |
| `TotalAmount` | `decimal` | 税込合計金額 |
| `PaymentMethod` | `PaymentMethod` | 支払方法 |
| `ReceivedAmount` | `decimal` | お預かり金額 |
| `ChangeAmount` | `decimal` | お釣り金額 |

静的ファクトリ: `Create(...)` — 店舗情報はハードコード（"Convenience POS Store", "東京都渋谷区〇〇1-2-3", "03-1234-5678"）

### PaymentResult (`src/Models/PaymentResult.cs`)
会計・決済結果（不変オブジェクト、privateコンストラクタ）。

| プロパティ | 型 | 説明 |
|---|---|---|
| `IsSuccess` | `bool` | 成功フラグ |
| `Transaction` | `SaleTransaction?` | 取引データ（成功時） |
| `ChangeAmount` | `decimal` | お釣り金額 |
| `ChangeBreakdown` | `CurrencyBreakdown?` | 金種内訳 |
| `ErrorMessage` | `string?` | エラーメッセージ（失敗時） |

静的ファクトリ: `Success(transaction, changeAmount)`, `Failed(errorMessage)`

### CurrencyBreakdown (`src/Models/CurrencyBreakdown.cs`)
お釣りの金種内訳。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Bill10000` | `int` | 1万円札 枚数 |
| `Bill5000` | `int` | 5千円札 枚数 |
| `Bill1000` | `int` | 千円札 枚数 |
| `Coin500` | `int` | 500円玉 枚数 |
| `Coin100` | `int` | 100円玉 枚数 |
| `Coin50` | `int` | 50円玉 枚数 |
| `Coin10` | `int` | 10円玉 枚数 |
| `Coin5` | `int` | 5円玉 枚数 |
| `Coin1` | `int` | 1円玉 枚数 |

greedy方式で各紙幣・硬貨の枚数を算出。`ToFormattedString()` で日本語フォーマット文字列を返す。

## 共通列挙型

### TaxRateType (`src/Common/TaxRateType.cs`)
```csharp
Reduced8 = 8   // 軽減税率 8% (飲食料品、新聞等)
Standard10 = 10 // 標準税率 10% (日用品、酒類、外食等)
```
拡張メソッド: `GetRateDecimal()`, `GetDisplayLabel()`

### PaymentMethod (`src/Common/PaymentMethod.cs`)
```csharp
Cash = 1              // 現金
CreditCard = 2        // クレジットカード
ElectronicMoney = 3   // 電子マネー
QrCode = 4            // QR・バーコード決済
```
拡張メソッド: `GetDisplayLabel()`, `GetIcon()`
