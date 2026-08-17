# SPEC-007: テスト

## 概要
SPEC-005（商品管理）とSPEC-006（売上集計）の品質保証ためのテスト仕様。

## テストフレームワーク
- xUnit 2.9.3
- Moq 4.20.72
- .NET 8.0-windows

## テスト戦略
- ユニットテスト: サービス層・ViewModel層のロジックを検証
- モック: リポジトリ層をMoqで置き換え、データベース依存を排除
- 統合テスト: SQLiteデータベースを使用したリポジトリ層の検証（既存）

## SPEC-005 テスト（ProductServiceSpec005Tests）
- AddProductAsync: 正常系（リポジトリ呼び出し確認）
- AddProductAsync: 異常系（null, 空コード, 空名, 負の単価）
- UpdateProductAsync: 正常系・異常系（null, ID=0）
- DeleteProductAsync: 正常系・異常系（無効ID）
- GetCategoriesAsync: リポジトリ委譲確認

## SPEC-005 テスト（ProductManagementViewModelSpec005Tests）
- InitializeAsync: 商品・カテゴリ読み込み
- SearchAsync: キーワードフィルタ
- ClearSearchCommand: キーワードリセット
- StartAddNew / CancelEdit: 編集状態遷移
- SaveProductAsync: 新規追加・更新・バリデーションエラー
- DeleteProductAsync: 削除実行・nullチェック
- HasSelectedProduct: 選択状態の切替
- SelectCategoryAsync: カテゴリ切替

## SPEC-006 テスト（AccountingServiceSpec006Tests）
- GetDailySalesSummaryAsync: 集計結果・空期間
- GetCategorySalesSummaryAsync: 集計結果・空期間
- GetPaymentMethodSalesSummaryAsync: 集計結果・空期間・委譲確認
- GetRecentTransactionsAsync: リポジトリ委譲確認

## SPEC-006 テスト（SalesReportViewModelSpec006Tests）
- InitializeAsync: 全サマリー読み込み・合計計算
- SetToday/SetThisWeek/SetThisMonth/SetLastMonth: 期間プリセット
- LoadReportAsync: バリデーション（開始日 > 終了日）
- LoadReportAsync: 合計計算・空データ時のゼロ表示
- Constructor: null検証

## テスト件数
- SPEC-005 関連: 約30テスト
- SPEC-006 関連: 約20テスト
- 既存テスト: 約76テスト
- **合計: 126テスト（全て合格）**

## 実行方法
```bash
dotnet test
```
