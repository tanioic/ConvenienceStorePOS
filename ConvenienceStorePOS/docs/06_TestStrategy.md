# 06_TestStrategy.md - テスト戦略

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

## テスト実行
```bash
dotnet test
```
