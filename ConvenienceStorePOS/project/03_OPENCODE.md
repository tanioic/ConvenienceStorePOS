# OPENCODE.md — Open Code専用設定

## 設計方針
- MVVMパターン（Model / ViewModel の実装）
- ライブラリ: CommunityToolkit.Mvvm を使用
- 制約: コードビハインド禁止
- テスト: xUnit によるテストを作成

## コーディングルール

### 基本方針
- **言語**: C# (.NET 8.0)
- **UI**: WPF (XAML)
- **設計パターン**: MVVM (CommunityToolkit.Mvvm)

### 制約事項
- MainWindow.xaml.cs には InitializeComponent() のみ許可
- すべてのロジックは ViewModel に実装
- XAML のビヘイビア/トリガーでバインディングを実現

### ViewModel ルール
- CommunityToolkit.Mvvm の [ObservableProperty] 属性を使用
- [RelayCommand] 属性でコマンドを定義
- ObservableObject を継承

### ファイル構成
```
src/
├── Common/       # 共通の列通の列挙型、拡張メソッド
├── Models/       # ドメインモデル（不変オブジェクト推奨）
├── Services/     # インタフェース + 実装（ビジネスロジック）
├── Data/         # Repository インタフェース + SQLite 実装
├── ViewModels/   # MVVM ViewModel
└── Converters/   # WPF IValueConverter
```

### 命名規則
| 種別 | 規則 | 例 |
|---|---|---|
| クラス名 | PascalCase | SaleService, CartItemViewModel |
| プロパティ | PascalCase | TotalAmount, ProductName |
| フィールド | _camelCase (private) | _saleService, _items |
| メソッド | PascalCase | AddProductByCodeAsync |
| インタフェース | I prefix | ISaleService, IProductRepository |
| 列挙型 | PascalCase | TaxRateType.Reduced8 |

### 非同期パターン
- async/Task パターンを使用
- メソッド名に Async サフィックス
- Repository 層の全メソッドは非同期

### エラーハンドリング
- ArgumentNullException で必須パラメータを検証
- DBトランザクションは BeginTransaction / Commit / Rollback で管理
- UIエラーは StatusMessage に表示（IsStatusError で色分け）

### テスト
- xUnit による単体テスト
- Moq で依存をモック化
- テストファイルは対応するソースファイルと同じディレクトリ構造
