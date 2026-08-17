# API.md — API仕様

## Repository インタフェース

### IProductRepository
- `GetByCodeAsync(code)` — JANコードで検索
- `GetByIdAsync(id)` — IDで取得
- `GetAllAsync()` — 全商品取得（IsActive=1, Category, Id 順）
- `GetByCategoryAsync(category)` — カテゴリ別取得
- `SearchAsync(keyword, category?)` — キーワード検索（Name LIKE / Code LIKE）
- `GetCategoriesAsync()` — カテゴリ一覧取得（DISTINCT）
- `AddAsync(product)` — 商品追加（last_insert_rowid でID取得）
- `UpdateAsync(product)` — 商品更新
- `DeleteAsync(id)` — 商品削除（論理削除: IsActive = false）

### ISaleRepository
- `SaveSaleAsync(sale, details)` — 取引ヘッダ＋明細をトランザクションで保存
- `GetByIdAsync(id)` — IDで取引取得
- `GetByTransactionNumberAsync(transactionNumber)` — 取引番号で取得
- `GetRecentSalesAsync(count=50)` — 最新取引を取得（Id DESC順）
- `GetDailySalesSummaryAsync(startDate, endDate)` — 日別売上集計
- `GetCategorySalesSummaryAsync(startDate, endDate)` — カテゴリ別売上集計
- `GetPaymentMethodSalesSummaryAsync(startDate, endDate)` — 支払方法別売上集計
- `GetSalesByDateRangeAsync(startDate, endDate)` — 期間指定売上取得

### IDatabaseInitializer
- `InitializeAsync()` — データベースとテーブルを初期化（シードデータ含む）

---

## Service インタフェース

### IProductService
- `GetAllProductsAsync()` — 全商品取得
- `GetProductByCodeAsync(code)` — JANコードで商品取得
- `SearchProductsAsync(keyword, category?)` — 商品検索
- `GetCategoriesAsync()` — カテゴリ一覧取得
- `AddProductAsync(product)` — 商品追加（バリデーション付き）
- `UpdateProductAsync(product)` — 商品更新（バリデーション付き）
- `DeleteProductAsync(id)` — 商品削除

### ISaleService
- `Summary` — SaleSummary プロパティ（現在のカートサマリー）
- `AddProductToCartAsync(product)` — カートに商品を追加（重複時は数量インクリメント）
- `UpdateQuantityAsync(index, quantity)` — 数量を更新
- `RemoveItemAsync(index)` — 明細を削除
- `ClearCart()` — カートをクリア
- `CartChanged` — カート変更イベント

### IAccountingService
- `ProcessPaymentAsync(cartItems, paymentMethod, receivedAmount, staffName, registerNumber)` — 決済処理
- `GetDailySalesSummaryAsync(startDate, endDate)` — 日別売上集計
- `GetCategorySalesSummaryAsync(startDate, endDate)` — カテゴリ別売上集計
- `GetPaymentMethodSalesSummaryAsync(startDate, endDate)` — 支払方法別売上集計
- `GetRecentTransactionsAsync(count)` — 最新取引取得

### IReceiptService
- `GenerateReceipt(saleTransaction, saleDetails)` — レシートオブジェクトを生成
- `GenerateReceiptText(receipt)` — レシートテキストを生成（32文字幅）

---

## DI登録一覧
```
IDatabaseInitializer     -> SqliteDatabaseInitializer    (Singleton)
IProductRepository       -> SqliteProductRepository      (Singleton)
ISaleRepository          -> SqliteSaleRepository         (Singleton)
IProductService          -> ProductService               (Singleton)
ISaleService             -> SaleService                  (Singleton)
IAccountingService       -> AccountingService            (Singleton)
IReceiptService          -> ReceiptService               (Singleton)
MainViewModel            -> MainViewModel                (Singleton)
MainWindow               -> MainWindow                   (Transient)
```
