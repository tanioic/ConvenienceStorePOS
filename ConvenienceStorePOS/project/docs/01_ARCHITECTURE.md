# ARCHITECTURE.md — システム構成

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

## アプリケーション起動フロー
1. `App.OnStartup` で DI コンテナを構築
2. `MainViewModel.InitializeAsync()` を呼び出し、DB初期化 + 商品ロード
3. `MainWindow` を表示

## NuGet パッケージ
| パッケージ | バージョン | 用途 |
|---|---|---|
| CommunityToolkit.Mvvm | 8.4.2 | MVVM基盤（ObservableProperty, RelayCommand） |
| Microsoft.Data.Sqlite | 10.0.11 | SQLiteデータアクセス |
| Microsoft.Extensions.DependencyInjection | 10.0.11 | 依存性注入 |
| Microsoft.Xaml.Behaviors.Wpf | 1.1.142 | XAMLビヘイビア |

---

## コーディングルール

### 基本方針
- **言語**: C# (.NET 8.0)
- **UI**: WPF (XAML)
- **設計パターン**: MVVM (CommunityToolkit.Mvvm)

### 制約事項

#### コードビハインド禁止（No Code-Behind）
- `MainWindow.xaml.cs` には `InitializeComponent()` のみ許可
- すべてのロジックは ViewModel に実装
- XAML のビヘイビア/トリガーでバインディングを実現

#### ViewModel ルール
- `CommunityToolkit.Mvvm` の `[ObservableProperty]` 属性を使用
- `[RelayCommand]` 属性でコマンドを定義
- `ObservableObject` を継承
- UIスレッドの `DispatcherTimer` はコンストラクタで初期化

### ファイル構成規則
```
src/
├── Common/       # 共通の列挙型、拡張メソッド
├── Models/       # ドメインモデル（不変オブジェクト推奨）
├── Services/     # インタフェース + 実装（ビジネスロジック）
├── Data/         # Repository インタフェース + SQLite 実装
├── ViewModels/   # MVVM ViewModel
└── Converters/   # WPF IValueConverter
```

### 命名規則
| 種別 | 規則 | 例 |
|---|---|---|
| クラス名 | PascalCase | `SaleService`, `CartItemViewModel` |
| プロパティ | PascalCase | `TotalAmount`, `ProductName` |
| フィールド | _camelCase (private) | `_saleService`, `_items` |
| メソッド | PascalCase | `AddProductByCodeAsync` |
| インタフェース | I prefix | `ISaleService`, `IProductRepository` |
| 列挙型 | PascalCase | `TaxRateType.Reduced8` |
| 定数 | PascalCase | `ReceiptWidth` |

### 非同期パターン
- `async/Task` パターンを使用
- メソッド名に `Async` サフィックス
- Repository 層の全メソッドは非同期

### エラーハンドリング
- `ArgumentNullException` で必須パラメータを検証
- DBトランザクションは `BeginTransaction` / `Commit` / `Rollback` で管理
- UIエラーは `StatusMessage` に表示（`IsStatusError` で色分け）

### テスト
- `xUnit` による単体テスト
- `Moq` で依存をモック化
- テストファイルは対応するソースファイルと同じディレクトリ構造

---

## Gitルール

### ブランチ戦略
- `main` — プロダクション穩定版
- `feature/*` — 新機能開発
- `bugfix/*` — バグ修正
- `refactor/*` — リファクタリング

### コミットメッセージ規則
- 日本語で記述
- 先頭にカテゴリプレフィックス
- 簡潔で具体的な内容

```
[feat] SPEC-003: 会計モーダルのテンキー入力機能を実装
[fix] バーコード入力時のNullReferenceExceptionを修正
[refactor] SaleServiceのスレッドセーフな処理に改善
[test] SPEC-002の税率別端数計算テストを追加
[docs] 03_DomainModel.mdを更新
```

### ファイル管理
- バイナリファイル、`bin/`、`obj/`、`.db` ファイルはコミットしない
- `*.csproj` のパッケージ参照はコミット対象
- XAML リソース（色定義等）は `App.xaml` に集約

### コードレビュー
- 機能単位でプルリクエストを作成
- テスト通過を確認
- コードビハインド禁止ルールの遵守を確認
