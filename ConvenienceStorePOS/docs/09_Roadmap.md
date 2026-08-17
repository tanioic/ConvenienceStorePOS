# 09_Roadmap.md - 開発ロードマップ

## 完了済みフェーズ

### Phase 1: 基盤構築 ✅
- [x] プロジェクト構成の作成 (.NET 8.0, WPF)
- [x] MVVM基盤の構築 (CommunityToolkit.Mvvm)
- [x] SQLiteデータベース設計と初期化
- [x] DIコンテナ設定 (`App.xaml.cs`)
- [x] 共通列挙型の定義 (`TaxRateType`, `PaymentMethod`)

### Phase 2: 商品登録 (SPEC-001) ✅
- [x] Product ドメインモデル
- [x] IProductRepository / SqliteProductRepository
- [x] IProductService / ProductService
- [x] バーコード/JANコード入力による商品登録
- [x] タッチ商品パネル（カテゴリ別グリッド表示）
- [x] 商品検索機能
- [x] カート操作（追加、数量変更、削除、全取消）
- [x] 25商品のシードデータ（5カテゴリ）

### Phase 3: 売上集計 (SPEC-002) ✅
- [x] SaleSummary 不変オブジェクト
- [x] インボイス制度準拠の税率別端数切り捨て計算
- [x] カート変更イベントによる自動更新
- [x] サマリー表示パネル（8%/10%内訳、税込合計）
- [x] 会計ボタンの有効/無効制御

### Phase 4: 会計・決済 (SPEC-003) ✅
- [x] 会計モーダル画面
- [x] 4種の支払方法（現金、クレジット、電子マネー、QR）
- [x] 現金テンキー + クイック金種ボタン
- [x] お釣り算出 + 金種内訳表示
- [x] IAccountingService / AccountingService
- [x] ISaleRepository / SqliteSaleRepository
- [x] Sales + SaleDetails テーブルへの永続化
- [x] 会計完了モーダル

### Phase 5: レシート (SPEC-004) ✅
- [x] Receipt / ReceiptLineItem 不変オブジェクト
- [x] IReceiptService / ReceiptService
- [x] 32文字幅テキストレシート生成
- [x] レシートプレビューモーダル
- [x] PrintDialog によるプリンタ出力
- [x] F2/F3 キーボードショートカット

### Phase 6: テスト (SPEC-007) ✅
- [x] Model ユニットテスト（Product, CartItem, SaleSummary, Receipt）
- [x] Service ユニットテスト（ProductService, SaleService）
- [x] Data ユニットテスト（SqliteProductRepository）
- [x] ViewModel ユニットテスト（MainViewModel, SaleSummaryViewModel）

---

## 未実装・今後の機能

### Phase 7: 商品管理 (SPEC-005) 🔄 部分実装
**状態**: Repository層の `AddAsync`, `UpdateAsync` は実装済。UI未実装。

- [ ] 商品管理画面（一覧表示、新規追加、編集、削除）
- [ ] 商品画像のサポート
- [ ] 在庫管理機能（入出荷、在庫アラート）
- [ ] カテゴリ管理機能

### Phase 8: 売上集計レポート (SPEC-006) ❌ 未実装
**状態**: `GetRecentSalesAsync` のみ実装。

- [ ] 日別売上集計画面
- [ ] 月別売上集計画面
- [ ] カテゴリ別売上集計
- [ ] 支払方法別集計
- [ ] CSV/Excelエクスポート機能
- [ ] グラフ表示（日別推移など）

### Phase 9: 運用強化（将来拡張）
- [ ] マルチレジ対応（レジ番号の設定変更）
- [ ] ログイン・権限管理（レジ担当者、店長）
- [ ] 在庫自動発注システム連携
- [ ] 割引・クーポン機能
- [ ] 返品・キャンセル処理
- [ ] 日次締め・レポート印刷
- [ ] ネットワーク対応（複数店舗、本部連携）

---

## 現状のコード規模
| カテゴリ | ファイル数 | 主要なファイル |
|---|---|---|
| Models | 8 | Product, CartItem, SaleTransaction, SaleDetail, SaleSummary, Receipt, PaymentResult, CurrencyBreakdown |
| Services | 8 | 4 Interfaces + 4 Implementations |
| Data | 6 | 3 Interfaces + 3 Implementations |
| ViewModels | 3 | MainViewModel (589行), ProductItemViewModel, CartItemViewModel |
| Converters | 1 | TaxRateBadgeBrushConverter, CurrencyFormatConverter |
| Common | 2 | TaxRateType, PaymentMethod |
| XAML | 1 | MainWindow.xaml (1202行) |
| Tests | 10 | Models(5) + Services(2) + Data(1) + ViewModels(2) |
