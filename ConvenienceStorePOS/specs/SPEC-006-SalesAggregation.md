# SPEC-006: 売上集計レポート

## 概要
日別・商品別・支払方法別の売上集計機能を提供する。経営判断に必要な売上データを可視化する。

## 範囲
- 日別売上集計（取引件数、販売点数、売上金額、消費税、現金/キャッシュレス内訳）
- 商品別（カテゴリ別）売上集計（税抜/税込/消費税）
- 支払方法別売上集計（現金、クレジットカード、電子マネー等）
- 期間指定での集計（日付ピッカー + クイック選択ボタン）

## 要件

### 6.1 期間指定
- 開始日と終了日をDatePickerで指定
- クイック選択ボタン: 今日、今週、今月、先月
- 集計実行ボタンでデータを取得

### 6.2 サマリー表示
- 売上合計（税込）
- 消費税合計
- 取引件数
- 販売点数
- 色分けで視覚的に強調

### 6.3 集計データ

#### 6.3.1 日別売上
- 日付、取引件数、販売点数、現金売上、キャッシュレス売上、売上合計、消費税

#### 6.3.2 商品別売上
- 商品名、販売点数、税抜売上、消費税、税込合計

#### 6.3.3 支払方法別売上
- 支払方法名、取引件数、売上合計

## 画面構成
- ヘッダー: 期間指定エリア（DatePicker + クイック選択 + 集計実行ボタン）
- サマリーパネル: 4つの集計値を横並びで表示
- DataGrid（左）: 日別売上一覧
- DataGrid（右上）: 商品別売上
- DataGrid（右下）: 支払方法別売上

## 実装ファイル
- `src/Data/ISaleRepository.cs` - GetDailySalesSummaryAsync, GetCategorySalesSummaryAsync, GetPaymentMethodSalesSummaryAsync, GetSalesByDateRangeAsync 追加
- `src/Data/SqliteSaleRepository.cs` - 各集計クエリ実装（GROUP BY 使用）
- `src/Services/IAccountingService.cs` - 集計メソッド追加
- `src/Services/AccountingService.cs` - 集計メソッド実装
- `src/ViewModels/SalesReportViewModel.cs` - 売上集計ViewModel
- `SalesReportWindow.xaml` - 売上集計画面
- `SalesReportWindow.xaml.cs` - コードビハインド
- `src/ViewModels/MainViewModel.cs` - OpenSalesReportCommand 追加
- `App.xaml.cs` - DI登録

## テスト
- `Services/AccountingServiceSpec006Tests.cs` - 集計メソッドの単体テスト
- `ViewModels/SalesReportViewModelSpec006Tests.cs` - ViewModel動作テスト

## 依存
- SPEC-002（売上データベース）
- SPEC-004（会計処理）
