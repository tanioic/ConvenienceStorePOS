# ConvenienceStorePOS

## プロジェクト概要
日本のコンビニエンスストアで実運用できる、軽減税率8% / 標準税率10%の二重税率制（インボイス制度）に対応したPOSレジシステム。

## 使用技術
| 項目 | 技術 |
|---|---|
| UIフレームワーク | WPF (.NET 8.0-windows) |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm 8.4.2) |
| データベース | SQLite (Microsoft.Data.Sqlite 10.0.11) |
| DIコンテナ | Microsoft.Extensions.DependencyInjection 10.0.11 |
| テストフレームワーク | xUnit 2.9.3 + Moq 4.20.72 |
| 対応OS | Windows 10/11 |

## 実装済み機能
| 機能 | 状態 | スペック |
|---|---|---|
| 商品登録（バーコード・タッチ） | 実装済 | SPEC-001 |
| 売上集計・税計算 | 実装済 | SPEC-002 |
| 会計・決済処理 | 実装済 | SPEC-003 |
| レシート発行 | 実装済 | SPEC-004 |
| 商品管理（CRUD） | 部分実装 | SPEC-005 |
| 売上集計レポート | 未実装 | SPEC-006 |
| 単体テスト | 実装済 | SPEC-007 |

## 開発手順

### 1. リポジトリのクローン
```bash
git clone <リポジトリURL>
cd ConvenienceStorePOS
```

### 2. ビルド
```bash
dotnet build
```

### 3. テスト実行
```bash
dotnet test
```

### 4. アプリケーション起動
```bash
dotnet run --project ConvenienceStorePOS
```

## ディレクトリ構成
```
ConvenienceStorePOS/
├── ConvenienceStorePOS/              # メイン WPF アプリケーション
│   ├── src/
│   │   ├── Common/                   # 共通列挙型・拡張メソッド
│   │   ├── Models/                   # ドメインモデル
│   │   ├── Services/                 # ビジネスロジック
│   │   ├── Data/                     # データアクセス
│   │   ├── ViewModels/               # MVVM ViewModel
│   │   └── Converters/               # WPF Value Converter
│   ├── App.xaml / App.xaml.cs        # アプリケーションエントリ + DI設定
│   └── MainWindow.xaml / .cs         # メイン画面
├── ConvenienceStorePOS.Tests/        # xUnit テストプロジェクト
└── project/
    ├── docs/                         # 仕様書
    │   ├── SPECIFICATION.md          # 全体仕様
    │   ├── ARCHITECTURE.md           # システム構成
    │   ├── DATABASE.md               # DB設計
    │   ├── API.md                    # API仕様
    │   ├── UI.md                     # 画面仕様
    │   └── TEST_PLAN.md              # テスト仕様
    ├── src/                          # ソースコード参照用
    ├── AGENTS.md                     # AIエージェント共通指示
    ├── OPENCODE.md                   # Open Code専用設定
    └── README.md                     # このファイル
```

## ドキュメント
- `project/docs/SPECIFICATION.md` — 全体仕様
- `project/docs/ARCHITECTURE.md` — システム構成
- `project/docs/DATABASE.md` — DB設計
- `project/docs/API.md` — API仕様
- `project/docs/UI.md` — 画面仕様
- `project/docs/TEST_PLAN.md` — テスト仕様
- `project/AGENTS.md` — AIエージェント共通指示
- `project/OPENCODE.md` — Open Code専用設定

## ライセンス
（未定）
