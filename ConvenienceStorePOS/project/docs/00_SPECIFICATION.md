# SPECIFICATION.md — 全体仕様

## プロジェクト名
**ConvenienceStorePOS** — コンビニエンスストア POS（Point of Sale）システム

## ビジョン
日本のコンビニエンスストアで実運用できる、軽減税率8% / 標準税率10%の二重税率制（インボイス制度）に対応したPOSレジシステムを構築する。

## 開発目的
1. レジ担当者がバーコードスキャンまたはタッチ操作で商品を迅速に登録できる
2. 日本の消費税制度（軽減税率8% / 標準税率10%）に準拠した正確な税額計算を実行する
3. 現金・クレジットカード・電子マネー・QR決済の全決済方法をサポートする
4. レシートのプレビュー表示とプリンタ出力を行う
5. 売上データをSQLiteデータベースに永続化し、取引履歴を確実に記録する

## 対象ユーザー
- コンビニエンスストアのレジ担当スタッフ
- 店舗マネージャー（売上集計データの参照）

## 技術スタック
| 項目 | 技術 |
|---|---|
| UIフレームワーク | WPF (.NET 8.0-windows) |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm 8.4.2) |
| データベース | SQLite (Microsoft.Data.Sqlite 10.0.11) |
| DIコンテナ | Microsoft.Extensions.DependencyInjection 10.0.11 |
| テストフレームワーク | xUnit 2.9.3 + Moq 4.20.72 |
| 対応OS | Windows 10/11 |

---

## 現状の実装状況
| 機能 | 状態 | スペック |
|---|---|---|
| 商品登商品登録（バーコード・タッチ） | **実装済** | SPEC-001 |
| 売上集計・税計算 | **実装済** | SPEC-002 |
| 会計・決済処理 | **実装済** | SPEC-003 |
| レシート発行 | **実装済** | SPEC-004 |
| 商品管理（CRUD） | **部分実装** | SPEC-005 |
| 売上集計レポート | **未実装** | SPEC-006 |
| 単体テスト | **実装済** | SPEC-007 |

---

## 機能一覧

### SPEC-001: 商品登録（売上登録） ✅ 実装済
- **バーコード/JANコード入力**: バーコードリーダーまたは手入力で商品をカートに追加
- **タッチ商品パネル**: カテゴリ別に商品ボタンを表示、1タップでカート追加
- **商品検索**: 商品名・JANコード・カテゴリでキーワード検索
- **カート操作**: 数量変更(+/-)、明細削除、全取消
- **同一商品重複登録**: 同じ商品を追加した場合、数量をインクリメント

### SPEC-002: 売上集計・明細確認 ✅ 実装済
- **SaleSummary計算**: カート内の全明細から合計を算出
- **インボイス制度対応税計算**: 税率区分ごとの対象合計金額に対して税率を乗せ端数切り捨て
- **8%軽減税率**: 飲食料品等（Reduced8）
- **10%標準税率**: 日用品等（Standard10）
- **会計ボタン制御**: カートに商品がある場合のみ有効化

### SPEC-003: 会計・決済処理 ✅ 実装済
- **現金決済**: テンキー入力 + クイック金種ボタン、お釣り自動算出 + 金種内訳表示
- **クレジットカード決済**: 全額カード決済（お釣りなし）
- **電子マネー決済**: 交通系IC, iD, QUICPay等
- **QR・バーコード決済**: PayPay, 楽天ペイ, d払い等
- **売上永続化**: Sales + SaleDetails テーブルにトランザクション保存
- **会計完了モーダル**: 取引番号、支払方法、お預かり、お釣りを表示

### SPEC-004: レシート発行 ✅ 実装済
- **レシートデータ構築**: 店舗情報、取引情報、明細、税集計、支払情報
- **テキストレシート生成**: 32文字幅のコンビニ形式レシート
- **画面プレビュー**: 等幅フォント（Consolas）で表示
- **プリンタ出力**: WPF PrintDialogによる印刷

### SPEC-005: 商品管理 🔄 部分実装
- **Repository層**: IProductRepository に AddAsync, UpdateAsync が定義済
- **Service層**: IProductService には管理系メソッドが未定義
- **UI**: 管理画面は未実装

### SPEC-006: 売上集計レポート ❌ 未実装
- **集計レポート機能**: 日別・月別の売上集計画面は未実装
- **Repository層**: GetRecentSalesAsync のみ実装済

### SPEC-007: テスト ✅ 実装済
- **Model ユニットテスト**: Product, CartItem, SaleSummary, Receipt
- **Service ユニットテスト**: ProductService, SaleService
- **Data ユニットテスト**: SqliteProductRepository
- **ViewModel ユニットテスト**: MainViewModel, SaleSummaryViewModel
- 合計126テスト（全て合格）

---

## 消費税計算仕様（インボイス制度準拠）
1. カート内の全明細を税率区分（Reduced8 / Standard10）で分類
2. 各税率区分の税抜合計金額を集計
3. 集計金額に対して税率を乗じ、Math.Floor で端数を切り捨てる
4. 明細行ごとの税額積み上げ方式とは異なる（インボイス制度の正式な端数処理方式）

## 支払方法
| 区分 | 値 | 説明 |
|---|---|---|
| Cash | 1 | 現金 |
| CreditCard | 2 | クレジットカード |
| ElectronicMoney | 3 | 電子マネー |
| QrCode | 4 | QR・バーコード決済 |

---

## ドメインモデル

### Product (src/Models/Product.cs)
商品マスタエンティティ。

| プロパティ | 型 | 説明 |
|---|---|---|
| Id | int | 商品ID（主キー） |
| Code | string | JANコード / 商品コード（一意） |
| Name | string | 商品名 |
| Price | decimal | 単価（税抜価格） |
| TaxRateType | TaxRateType | 消費税区分（Reduced8 / Standard10） |
| Category | string | カテゴリ名 |
| StockQuantity | int | 在庫数（デフォルト: 100） |
| IsActive | bool | 有効フラグ（デフォルト: true） |

算出プロパティ:
- TaxRate -> TaxRateType.GetRateDecimal() (0.08m or 0.10m)
- PriceWithTax -> Math.Floor(Price * (1m + TaxRate))

### CartItem (src/Models/CartItem.cs)
カート・売上明細行。Product と数量を保持。

| プロパティ | 型 | 説明 |
|---|---|---|
| Product | Product | 参照商品 |
| Quantity | int | 数量（最低1） |

委譲プロパティ: ProductId, ProductCode, ProductName, UnitPrice, TaxRateType, TaxRate

算出プロパティ:
- SubtotalExcludingTax -> UnitPrice * Quantity
- TaxAmount -> Math.Floor(SubtotalExcludingTax * TaxRate)
- SubtotalIncludingTax -> SubtotalExcludingTax + TaxAmount

コンストラクタで product が null の場合は ArgumentNullException、quantity が1未満の場合は1にクランプ。

### SaleSummary (src/Models/SaleSummary.cs)
売上サマリー（不変オブジェクト）。税率別端数切り捨て方式で税額を計算。

| プロパティ | 型 | 説明 |
|---|---|---|
| TotalQuantity | int | 合計点数 |
| SubtotalExcludingTax | decimal | 税抜合計金額 |
| Reduced8TaxableAmount | decimal | 8%対象税抜金額 |
| Reduced8TaxAmount | decimal | 8%消費税額（Floor） |
| Standard10TaxableAmount | decimal | 10%対象税抜金額 |
| Standard10TaxAmount | decimal | 10%消費税額（Floor） |
| TotalTaxAmount | decimal | 消費税合計（8% + 10%） |
| TotalAmount | decimal | 税込合計（税抜合計 + 消費税合計） |

静的プロパティ: SaleSummary.Empty (全値が0)

### SaleTransaction (src/Models/SaleTransaction.cs)
売上取引データ（ヘッダ）。DB永続化対象。

| プロパティ | 型 | 説明 |
|---|---|---|
| Id | int | データベース主キー |
| TransactionNumber | string | 取引番号（例: TRX-20260817123456-123） |
| CreatedAt | DateTime | 取引日時 |
| TotalQuantity | int | 合計点数 |
| SubtotalExcludingTax | decimal | 税抜合計 |
| Reduced8TaxableAmount | decimal | 8%対象税抜額 |
| Reduced8TaxAmount | decimal | 8%消費税額 |
| Standard10TaxableAmount | decimal | 10%対象税抜額 |
| Standard10TaxAmount | decimal | 10%消費税額 |
| TotalTaxAmount | decimal | 消費税合計 |
| TotalAmount | decimal | 税込合計金額 |
| PaymentMethod | PaymentMethod | 支払方法区分 |
| ReceivedAmount | decimal | お預かり金額 |
| ChangeAmount | decimal | お釣り金額 |
| StaffName | string | レジ担当者名 |
| RegisterNumber | string | レジ番号 |
| Details | List<SaleDetail> | 取引明細リスト |

静的メソッド: GenerateTransactionNumber() -> TRX-{yyyyMMddHHmmss}-{random 100-999}

### SaleDetail (src/Models/SaleDetail.cs)
売上取引明細データ。DB永続化対象。

| プロパティ | 型 | 説明 |
|---|---|---|
| Id | int | データベース主キー |
| SaleId | int | 取引ヘッダID（外部キー） |
| ProductId | int | 商品ID |
| ProductCode | string | JANコード |
| ProductName | string | 商品名 |
| UnitPrice | decimal | 単価（税抜） |
| TaxRateType | TaxRateType | 税率区分 |
| Quantity | int | 数量 |

静的ファクトリ: FromCartItem(CartItem item, int saleId = 0)

### Receipt (src/Models/Receipt.cs)
レシートデータ（不変オブジェクト）。ReceiptLineItem を内包。

| プロパティ | 型 | 説明 |
|---|---|---|
| StoreName | string | 店舗名 |
| StoreAddress | string | 店舗住所 |
| StorePhone | string | 電話番号 |
| RegisterNumber | string | レジ番号 |
| StaffName | string | 担当者名 |
| TransactionNumber | string | 取引番号 |
| TransactionDateTime | DateTime | 取引日時 |
| LineItems | IReadOnlyList<ReceiptLineItem> | 明細行リスト |
| TotalQuantity | int | 合計点数 |
| SubtotalExcludingTax | decimal | 税抜合計 |
| Reduced8TaxableAmount | decimal | 8%対象税抜額 |
| Reduced8TaxAmount | decimal | 8%消費税額 |
| Standard10TaxableAmount | decimal | 10%対象税抜額 |
| Standard10TaxAmount | decimal | 10%消費税額 |
| TotalTaxAmount | decimal | 消費税合計 |
| TotalAmount | decimal | 税込合計金額 |
| PaymentMethod | PaymentMethod | 支払方法 |
| ReceivedAmount | decimal | お預かり金額 |
| ChangeAmount | decimal | お釣り金額 |

静的ファクトリ: Create(...) — 店舗情報はハードコード（"Convenience POS Store", "東京都渋谷区〇〇1-2-3", "03-1234-5678"）

### PaymentResult (src/Models/PaymentResult.cs)
会計・決済結果（不変オブジェクト、privateコンストラクタ）。

| プロパティ | 型 | 説明 |
|---|---|---|
| IsSuccess | bool | 成功フラグ |
| Transaction | SaleTransaction? | 取引データ（成功時） |
| ChangeAmount | decimal | お釣り金額 |
| ChangeBreakdown | CurrencyBreakdown? | 金種内訳 |
| ErrorMessage | string? | エラーメッセージ（失敗時） |

静的ファクトリ: Success(transaction, changeAmount), Failed(errorMessage)

### CurrencyBreakdown (src/Models/CurrencyBreakdown.cs)
お釣りの金種内訳。

| プロパティ | 型 | 説明 |
|---|---|---|
| Bill10000 | int | 1万円札 枚数 |
| Bill5000 | int | 5千円札 枚数 |
| Bill1000 | int | 千円札 枚数 |
| Coin500 | int | 500円玉 枚数 |
| Coin100 | int | 100円玉 枚数 |
| Coin50 | int | 50円玉 枚数 |
| Coin10 | int | 10円玉 枚数 |
| Coin5 | int | 5円玉 枚数 |
| Coin1 | int | 1円玉 枚数 |

greedy方式で各紙幣・硬貨の枚数を算出。ToFormattedString() で日本語フォーマット文字列を返す。

## 共通列挙型

### TaxRateType (src/Common/TaxRateType.cs)
`csharp
Reduced8 = 8   // 軽減税率 8% (飲食料品、新聞等)
Standard10 = 10 // 標準税率 10% (日用品、酒類、外食等)
`
拡張メソッド: GetRateDecimal(), GetDisplayLabel()

### PaymentMethod (src/Common/PaymentMethod.cs)
`csharp
Cash = 1              // 現金
CreditCard = 2        // クレジットカード
ElectronicMoney = 3   // 電子マネー
QrCode = 4            // QR・バーコード決済
`
拡張メソッド: GetDisplayLabel(), GetIcon()


---

## SPEC-001 詳細: 商品登録（売上登録）仕様

### 商品マスタおよび永続化
- SQLiteデータベース（pos.db）に商品マスタを保持する。
- 商品は以下の属性を持つ：Id, Code/JANCode, Name, Price, TaxRateType, Category, StockQuantity, IsActive
- アプリケーション初回起動時にデータベースおよびテーブルを自動生成し、代表的なコンビニ商品マスタをシードデータとして初期登録する。

### 商品の登録方法
1. バーコード / JANコード入力: バーコードリーダーからの入力または手入力。Enterキー押下または「登録」ボタン押下でカートへ追加。登録後、入力欄は自動クリア。
2. タッチ商品パネル (Quick Touch Panel): カテゴリ別タブで商品をフィルタ表示。商品ボタンで1アクションでカートへ追加。
3. 商品検索機能 (Product Search): 商品名・JANコード・カテゴリでキーワード検索。検索結果から対象商品を選択して登録可能。

### カートおよび取引明細操作
- 同一商品の重複登録: 既にカートに存在する商品を再登録した場合、数量を+1する。
- 数量変更: 各明細行の+/-ボタンで数量変更。数量が0以下になる場合は明細を削除。
- 行取消（明細削除）: 対象の明細行の「取消」ボタンによりカートから除外。
- 全取消（カートクリア）: 「全取消」操作によりカート内の全明細をリセット。

### 金額および日本の消費税制計算
- 軽減税率制度（8%）: 食品・飲料・おにぎり・弁当等に適用。
- 標準税率制度（10%）: 日用品・雑貨・酒類等に適用。
- 各明細の計算: 税抜小計 = 単価（税抜） x 数量 / 税込小計 = 税抜小計 + 消費税額（端数切り捨て: Floor）
- 取引合計（サマリー）の計算: 合計点数、税抜合計額、8%対象額/消費税額、10%対象額/消費税額、税込合計金額

### 非機能要件
- 設計パターン: MVVM（Model-View-ViewModel）パターンに厳格に従う。
- MVVMライブラリ: CommunityToolkit.Mvvm を採用。
- 制約事項: コードビハインド禁止（No Code-Behind）。
- データアクセス: Repositoryパターンと Microsoft.Data.Sqlite によるSQLiteアクセス。
- ビジネスロジック層: IProductService, ISaleService による分離。
- 依存性注入 (DI): Microsoft.Extensions.DependencyInjection を使用。
- 単体テスト: xUnit による包括的なテストを作成。

---

## SPEC-002 詳細: 売上集計・明細確認仕様

### 売上サマリー計算
1. 合計点数: TotalQuantity = 全明細の数量合計
2. 税抜合計金額: SubtotalExcludingTax = 全明細の税抜小計の合計
3. 軽減税率8%: Reduced8TaxableAmount = 8%対象明細の税抜小計合計 / Reduced8TaxAmount = Floor(Reduced8TaxableAmount x 0.08)
4. 標準税率10%: Standard10TaxableAmount = 10%対象明細の税抜小計合計 / Standard10TaxAmount = Floor(Standard10TaxableAmount x 0.10)
5. 消費税合計・税込合計: TotalTaxAmount = 8% + 10% / TotalAmount = 税抜合計 + 消費税合計

### 明細行の税計算
各明細行（CartItem）においても個別に税額を保持する（レシート印刷等のため）。
- CartItem.SubtotalExcludingTax = UnitPrice x Quantity
- CartItem.TaxAmount = Floor(SubtotalExcludingTax x TaxRate)
- CartItem.SubtotalIncludingTax = SubtotalExcludingTax + TaxAmount
- ※この行単位の税額は参照用であり、合計税額の算出には使用しない

### 売上サマリー表示パネル
| 表示項目 | 内容 |
|---|---|
| 合計点数 | TotalQuantity 点 |
| 税抜合計 | SubtotalExcludingTax 円 |
| 8%対象 | Reduced8TaxableAmount 円（税額: Reduced8TaxAmount 円） |
| 10%対象 | Standard10TaxableAmount 円（税額: Standard10TaxAmount 円） |
| 消費税合計 | TotalTaxAmount 円 |
| 税込合計 | TotalAmount 円（強調表示） |

### 会計ボタンの制御
- カートに1件以上の商品が存在し、かつ TotalAmount > 0 の場合にのみ「会計へ」ボタンを有効化。
- CanOpenAccounting = HasCartItems AND TotalAmount > 0

### カートの変更検知と自動更新
- カートへの商品追加・削除・数量変更のたびに SaleService.CartChanged イベントが発火される。
- MainViewModel はこのイベントをサブスクライブし、SaleSummary の再計算と画面バインディング値の更新を自動で行う。

---

## SPEC-003 詳細: 会計・決済処理仕様

### 支払方法の選択
- 現金 (Cash): お預かり金額を入力し、お釣りを自動算出。テンキー（0-9, 00, クリア）およびクイック金種ボタン（「ちょうど」「1,000円」「5,000円」「10,000円」「+1,000円」等）による高速入力。お預かり金額が税込合計金額未満の場合は決済確定不可。
- クレジットカード (Credit Card): 全額カード決済（お預かり金額＝合計金額、お釣り＝0円）。
- 電子マネー (Electronic Money): 交通系IC, iD, QUICPay等による全額決済。
- QR・バーコード決済 (QR / Barcode Pay): PayPay, 楽天ペイ, d払い, au PAY等による全額決済。

### お預かり・お釣り計算
- お釣り計算式: お釣り = お預かり金額 - 税込合計金額
- 金種内訳算出: 10,000円札、5,000円札、1,000円札、500円玉、100円玉、50円玉、10円玉、5円玉、1円玉の枚数を自動算出。

### 売上トランザクションの永続化
- Sales テーブル（取引ヘッダ）: TransactionNumber, CreatedAt, TotalQuantity, 各税額、TotalAmount, PaymentMethod, ReceivedAmount, ChangeAmount, StaffName, RegisterNumber
- SaleDetails テーブル（取引明細）: SaleId, ProductId, ProductCode, ProductName, UnitPrice, TaxRateType, Quantity

### 会計完了と次客リセット
- 会計確定後、完了画面（支払方法、お預かり、お釣り、取引番号）を表示。
- 「次の取引へ」ボタン押下により、カートおよび会計状態を即座にリセットし、初期待機状態へ遷移。

---

## SPEC-004 詳細: レシート発行仕様

### レシートデータ構築
- 店舗情報: StoreName, StoreAddress, StorePhone, RegisterNumber, StaffName
- 取引情報: TransactionNumber, TransactionDateTime
- 明細情報: 商品名、数量、単価（税抜）、税区分表示（※8% / ※10%）、税込小計
- 金額集計: 税抜合計、8%対象額/消費税額、10%対象額/消費税額、税込合計金額
- 支払情報: 支払方法、お預かり金額、お釣り金額

### レシートテキスト生成
- 1行あたり 32文字幅（コンビニ一般的なレシート幅）
- 区切り線: =（ヘッダ/フッタ）, -（セクション区切り）
- 金額: 右寄せフォーマット（¥xxx）
- 税区分: 商品名末尾に ※8% / ※10% を付記
- 数量: 商品名 数量 ¥小計 形式

### レシートプレビューモーダル
- モーダルタイトル: "レシートプレビュー"
- レシートテキストを等幅フォント（Consolas / Courier New）で表示
- 印刷ボタン: レシートプリンタへの出力
- 閉じるボタン: モーダルを閉じ、次客待機状態へ遷移
- ESCキーでモーダルを閉じる

### レシート印刷
- PrintDialog を使用し、システム設定されたプリンタに出力。
- プリンタ未設定時はエラーメッセージを表示し、プレビュー画面は維持。

### 会計完了フロー統合
1. 会計確定 -> 取引完了モーダル表示
2. 「レシートを表示」ボタン押下 -> レシートプレビューモーダル表示
3. プレビュー画面で印刷ボタン押下 -> プリンタ出力
4. 「閉じる」ボタン押下 -> モーダル閉じ・次客リセット

---

## SPEC-005 詳細: 商品管理（CRUD）

### 範囲
- 商品の一覧表示、検索、フィルタ
- 商品の新規追加
- 商品情報の編集
- 商品の削除（論理削除）

### 要件
#### 商品一覧
- 商品一覧をDataGridに表示する
- カテゴリでフィルタできる
- キーワードで検索できる
- 選択した商品を表示する

#### 商品追加
- JANコード（必須、重複不可）
- 商品名（必須）
- 単価（税抜、0以上）
- 消費税区分（標準10% / 軽減8%）
- カテゴリ（必須）
- 在庫数

#### 商品編集
- 選択した商品の情報を編集する
- 編集中は右パネルにフォームを表示する
- 保存時にバリデーションを行う

#### 商品削除
- 選択した商品を論理削除する（IsActive = false）
- 削除前に確認メッセージを表示する

### 画面構成
- 左側: 商品一覧DataGrid + 検索・フィルタバー
- 右側: 操作ボタン / 編集フォーム（トグル表示）
- ヘッダー: 「商品管理」ボタン（メイン画面からアクセス）

### 実装ファイル
- src/Data/IProductRepository.cs - DeleteAsync(int id) 追加
- src/Data/SqliteProductRepository.cs - DeleteAsync実装（論理削除）
- src/Services/IProductService.cs - AddProductAsync, UpdateProductAsync, DeleteProductAsync 追加
- src/Services/ProductService.cs - 各メソッド実装（バリデーション付き）
- src/ViewModels/ProductManagementViewModel.cs - CRUD操作ViewModel
- ProductManagementWindow.xaml - 商品管理画面
- ProductManagementWindow.xaml.cs - コードビハインド
- src/ViewModels/MainViewModel.cs - OpenProductManagementCommand 追加
- App.xaml.cs - DI登録

### テスト
- Services/ProductServiceSpec005Tests.cs - バリデーション・リポジトリ委譲テスト
- ViewModels/ProductManagementViewModelSpec005Tests.cs - ViewModel動作テスト

---

## SPEC-006 詳細: 売上集計レポート

### 範囲
- 日別売上集計（取引件数、販売点数、売上金額、消費税、現金/キャッシュレス内訳）
- 商品別（カテゴリ別）売上集計（税抜/税込/消費税）
- 支払方法別売上集計（現金、クレジットカード、電子マネー等）
- 期間指定での集計（日付ピッカー + クイック選択ボタン）

### 要件
#### 期間指定
- 開始日と終了日をDatePickerで指定
- クイック選択ボタン: 今日、今週、今月、先月
- 集計実行ボタンでデータを取得

#### サマリー表示
- 売上合計（税込）
- 消費税合計
- 取引件数
- 販売点数
- 色分けで視覚的に強調

#### 集計データ
- 日別売上: 日付、取引件数、販売点数、現金売上、キャッシュレス売上、売上合計、消費税
- 商品別売上: 商品名、販売点数、税抜売上、消費税、税込合計
- 支払方法別売上: 支払方法名、取引件数、売上合計

### 画面構成
- ヘッダー: 期間指定エリア（DatePicker + クイック選択 + 集計実行ボタン）
- サマリーパネル: 4つの集計値を横並びで表示
- DataGrid（左）: 日別売上一覧
- DataGrid（右上）: 商品別売上
- DataGrid（右下）: 支払方法別売上

### 実装ファイル
- src/Data/ISaleRepository.cs - GetDailySalesSummaryAsync, GetCategorySalesSummaryAsync, GetPaymentMethodSalesSummaryAsync, GetSalesByDateRangeAsync 追加
- src/Data/SqliteSaleRepository.cs - 各集計クエリ実装（GROUP BY 使用）
- src/Services/IAccountingService.cs - 集計メソッド追加
- src/Services/AccountingService.cs - 集計メソッド実装
- src/ViewModels/SalesReportViewModel.cs - 売上集計ViewModel
- SalesReportWindow.xaml - 売上集計画面
- SalesReportWindow.xaml.cs - コードビハインド
- src/ViewModels/MainViewModel.cs - OpenSalesReportCommand 追加
- App.xaml.cs - DI登録

### テスト
- Services/AccountingServiceSpec006Tests.cs - 集計メソッドの単体テスト
- ViewModels/SalesReportViewModelSpec006Tests.cs - ViewModel動作テスト
