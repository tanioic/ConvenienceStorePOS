# 02_Architecture.md - システムアーキテクチャ

## 全体構成
```
ConvenienceStorePOS.sln
├── ConvenienceStorePOS/              # メイン WPF アプリケーション
│   ├── src/
│   │   ├── Common/                   # 共通列挙型・拡張メソッド
│   │   ├── Models/                   # ドメインモデル
│   │   ├── Services/                 # ビジネスロジック（インタフェース + 実装）
│   │   ├── Data/                     # データアクセス（Repository + DB初期化）
│   │   ├── ViewModels/               # MVVM ViewModel
│   │   └── Converters/               # WPF Value Converter
│   ├── specs/                        # スペックドキュメント (SPEC-001〜007)
│   ├── docs/                         # 仕様書 (00〜09)
│   ├── App.xaml / App.xaml.cs        # アプリケーションエントリ + DI設定
│   └── MainWindow.xaml / .cs         # メイン画面 (XAML のみ、コードビハインド禁止)
└── ConvenienceStorePOS.Tests/        # xUnit テストプロジェクト
    ├── Models/
    ├── Services/
    ├── Data/
    └── ViewModels/
```

## 設計パターン

### MVVM (Model-View-ViewModel)
- **Model**: `Product`, `CartItem`, `SaleTransaction`, `SaleDetail`, `SaleSummary`, `Receipt`, `PaymentResult`, `CurrencyBreakdown`
- **ViewModel**: `MainViewModel`, `ProductItemViewModel`, `CartItemViewModel`
- **View**: `MainWindow.xaml` (コードビハインド禁止)

### Repository パターン
- `IProductRepository` → `SqliteProductRepository`
- `ISaleRepository` → `SqliteSaleRepository`
- `IDatabaseInitializer` → `SqliteDatabaseInitializer`

### Service パターン
- `IProductService` → `ProductService` (商品検索・取得)
- `ISaleService` → `SaleService` (カート操作)
- `IAccountingService` → `AccountingService` (会計・決済)
- `IReceiptService` → `ReceiptService` (レシート生成)

## 依存性注入 (DI)
`App.xaml.cs` の `ConfigureServices` メソッドで以下を登録:
```csharp
// Data Layer
services.AddSingleton<IDatabaseInitializer, SqliteDatabaseInitializer>();
services.AddSingleton<IProductRepository, SqliteProductRepository>();
services.AddSingleton<ISaleRepository, SqliteSaleRepository>();

// Business Logic Services
services.AddSingleton<IProductService, ProductService>();
services.AddSingleton<ISaleService, SaleService>();
services.AddSingleton<IAccountingService, AccountingService>();
services.AddSingleton<IReceiptService, ReceiptService>();

// ViewModels
services.AddSingleton<MainViewModel>();

// Views
services.AddTransient<MainWindow>();
```

## データフロー
```
User Action (XAML)
    ↓
MainViewModel (Command / Property)
    ↓
ISaleService / IProductService / IAccountingService / IReceiptService
    ↓
IProductRepository / ISaleRepository
    ↓
SQLite (pos.db)
```

## NuGet パッケージ
| パッケージ | バージョン | 用途 |
|---|---|---|
| CommunityToolkit.Mvvm | 8.4.2 | MVVM基盤（ObservableProperty, RelayCommand） |
| Microsoft.Data.Sqlite | 10.0.11 | SQLiteデータアクセス |
| Microsoft.Extensions.DependencyInjection | 10.0.11 | 依存性注入 |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.142 | XAMLビヘイビア |

## アプリケーション起動フロー
1. `App.OnStartup` で DI コンテナを構築
2. `MainViewModel.InitializeAsync()` を呼び出し、DB初期化 + 商品ロード
3. `MainWindow` を表示
