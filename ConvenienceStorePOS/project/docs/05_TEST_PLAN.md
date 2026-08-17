# TEST_PLAN.md — テスト仕様

## テストフレームワーク
- **xUnit** 2.9.3
- **Moq** 4.20.72（モックライブラリ）
- **coverlet.collector** 6.0.4（カバレッジ）
- **対象フレームワーク**: .NET 8.0-windows

## テストプロジェクト
```
ConvenienceStorePOS.Tests/
├── ConvenienceStorePOS.Tests.csproj
├── UnitTest1.cs                     # 基本テスト
├── Models/
│   ├── ProductTests.cs              # Product モデルテスト
│   ├── CartItemTests.cs             # CartItem モデルテスト
│   ├── SaleSummaryTests.cs          # SaleSummary 税計算テスト
│   ├── SaleSummarySpec002Tests.cs   # SPEC-002 対応テスト
│   └── ReceiptTests.cs              # Receipt モデルテスト
├── Services/
│   ├── ProductServiceTests.cs       # ProductService テスト
│   └── SaleServiceTests.cs          # SaleService テスト
├── Data/
│   └── SqliteProductRepositoryTests.cs  # SQLite リポジトリテスト
└── ViewModels/
    ├── MainViewModelTests.cs        # MainViewModel テスト
    └── SaleSummaryViewModelSpec002Tests.cs  # SPEC-002 ViewModelテスト
```

## テストカバレッジ対象

### Model層テスト
- **ProductTests**: Product のプロパティ、TaxRate、PriceWithTax の計算
- **CartItemTests**: CartItem のコンストラクタ検証、税額計算、数量クランプ
- **SaleSummaryTests / SaleSummarySpec002Tests**:
  - 空カートの SaleSummary が全0を返すこと
  - 軽減税率のみのパターン
  - 標準税率のみのパターン
  - 軽減税率と標準税率の混在パターン
  - 端数が Math.Floor で切り捨てられること（インボイス制度準拠）
- **ReceiptTests**: Receipt オブジェクトの正しい構築

### Service層テスト
- **ProductServiceTests**: 商品検索、カテゴリ取得
- **SaleServiceTests**: カート操作（追加、重複追加時の数量インクリメント、削除、クリア）

### Data層テスト
- **SqliteProductRepositoryTests**: SQLite での商品CRUD操作

### ViewModel層テスト
- **MainViewModelTests**: メインViewModelの操作
- **SaleSummaryViewModelSpec002Tests**: SPEC-002対応のサマリー表示テスト

---

## SPEC-005 テスト（商品管理）

### ProductServiceSpec005Tests
- AddProductAsync: 正常系（リポジトリ呼び出し確認）
- AddProductAsync: 異常系（null, 空コード, 空名, 負の単価）
- UpdateProductAsync: 正常系・異常系（null, ID=0）
- DeleteProductAsync: 正常系・異常系（無効ID）
- GetCategoriesAsync: リポジトリ委譲確認

### ProductManagementViewModelSpec005Tests
- InitializeAsync: 商品・カテゴリ読み込み
- SearchAsync: キーワードフィルタ
- ClearSearchCommand: キーワードリセット
- StartAddNew / CancelEdit: 編集状態遷移
- SaveProductAsync: 新規追加・更新・バリデーションエラー
- DeleteProductAsync: 削除実行・nullチェック
- HasSelectedProduct: 選択状態の切替
- SelectCategoryAsync: カテゴリ切替

---

## SPEC-006 テスト（売上集計）

### AccountingServiceSpec006Tests
- GetDailySalesSummaryAsync: 集計結果・空期間
- GetCategorySalesSummaryAsync: 集計結果・空期間
- GetPaymentMethodSalesSummaryAsync: 集計結果・空期間・委譲確認
- GetRecentTransactionsAsync: リポジトリ委譲確認

### SalesReportViewModelSpec006Tests
- InitializeAsync: 全サマリー読み込み・合計計算
- SetToday/SetThisWeek/SetThisMonth/SetLastMonth: 期間プリセット
- LoadReportAsync: バリデーション（開始日 > 終了日）
- LoadReportAsync: 合計計算・空データ時のゼロ表示
- Constructor: null検証

---

## テスト件数
- SPEC-005 関連: 約30テスト
- SPEC-006 関連: 約20テスト
- 既存テスト: 約76テスト
- **合計: 126テスト（全て合格）**

## テスト実行
```bash
dotnet test
```
