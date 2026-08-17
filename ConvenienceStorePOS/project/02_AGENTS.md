# AGENTS.md — AIエージェント共通指示

## プロジェクト概要
**ConvenienceStorePOS** — コンビニエンスストア POS（Point of Sale）システム

日本のコンビニエンスストアで実運用できる、軽減税率8% / 標準税率10%の二重税率制（インボイス制度）に対応したPOSレジシステム。

## 技術スタック
| 項目 | 技術 |
|---|---|
| UIフレームワーク | WPF (.NET 8.0-windows) |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm 8.4.2) |
| データベース | SQLite (Microsoft.Data.Sqlite 10.0.11) |
| DIコンテナ | Microsoft.Extensions.DependencyInjection 10.0.11 |
| テストフレームワーク | xUnit 2.9.3 + Moq 4.20.72 |

## コーディング規約

### 基本方針
- **言語**: C# (.NET 8.0)
- **UI**: WPF (XAML)
- **設計パターン**: MVVM (CommunityToolkit.Mvvm)
- **コードビハインド禁止**: MainWindow.xaml.cs には InitializeComponent() のみ許可

### ViewModel ルール
- CommunityToolkit.Mvvm の [ObservableProperty] 属性を使用
- [RelayCommand] 属性でコマンドを定義
- ObservableObject を継承

### 命名規則
| 種別 | 規則 | 例 |
|---|---|---|
| クラス名 | PascalCase | SaleService, CartItemViewModel |
| プロパティ | PascalCase | TotalAmount, ProductName |
| フィールド | _camelCase (private) | _saleService, _items |
| メソッド | PascalCase | AddProductByCodeAsync |
| インタフェース | I prefix | ISaleService, IProductRepository |
| 列挙型 | PascalCase | TaxRateType.Reduced8 |

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

## 開発手順

### 既に実装済みの機能
1. **SPEC-001 商品登録**: バーコード入力、タッチ商品パネル、商品検索、カート操作
2. **SPEC-002 売上集計**: インボイス制度対応の税率別端数切り捨て計算、サマリー表示
3. **SPEC-003 会計・決済**: 現金/クレジット/電子マネー/QP決済、お釣り算出、DB永続化
4. **SPEC-004 レシート**: テキストレシート生成、画面プレビュー、プリンタ出力
5. **SPEC-007 テスト**: 126テスト（全合格）

### 部分実装済みの機能
- **SPEC-005 商品管理**: Repository層のみ実装、Service層・UIは未実装

### 未実装の機能
- **SPEC-006 売上集計レポート**: Repository層のGetRecentSalesAsyncのみ実装

## テスト
```bash
dotnet test
```

## ドキュメント
- `docs/SPECIFICATION.md` — 全体仕様（プロジェクトビジョン、機能一覧、ドメインモデル、各SPEC詳細）
- `docs/ARCHITECTURE.md` — システム構成、設計パターン、DI、コーディングルール、Gitルール
- `docs/DATABASE.md` — DB設計（テーブル定義、シードデータ）
- `docs/API.md` — Repository/Service インタフェース仕様
- `docs/UI.md` — 画面仕様（レイアウト、モーダル、キーボードショートカット）
- `docs/TEST_PLAN.md` — テスト仕様（テスト戦略、カバレッジ対象）

## 注意事項
- コードビハインド禁止ルールを厳守すること
- ViewModel のプロパティは [ObservableProperty] 属性を使用すること
- Repository 層の全メソッドは非同期（async/Task）であること
- DBトランザクションは BeginTransaction / Commit / Rollback で管理すること
- テストは xUnit + Moq で作成すること
