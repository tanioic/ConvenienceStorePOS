# SPEC-002: 売上集計・明細確認（Sales Summary & Cart Confirmation）仕様書

## 1. 概要 (Overview)
コンビニエンスストアPOSシステムにおける「売上集計・明細確認」機能の仕様を定義する。
SPEC-001で登録されたカート内の取引明細に対し、日本のインボイス制度（税率別端数切り捨て方式）に基づく正確な消費税計算・合計金額算出を行い、会計（SPEC-003）へ引き渡す前の最終確認パネルを提供する。

---

## 2. 業務要件 (Functional Requirements)

### 2.1 売上サマリー計算（SaleSummary Calculation）
カート内の全取引明細（`CartItem`）から、以下の売上サマリー（`SaleSummary`）を算出する。

#### 2.1.1 合計点数（Total Items Count）
- `TotalQuantity` = 全明細の数量合計

#### 2.1.2 税抜合計金額（Subtotal Excluding Tax）
- `SubtotalExcludingTax` = 全明細の税抜小計（`UnitPrice x Quantity`）の合計

#### 2.1.3 軽減税率8%の計算（Reduced Rate 8% Calculation）
- `Reduced8TaxableAmount` = 軽減税率（`TaxRateType.Reduced8`）対象明細の税抜小計合計
- `Reduced8TaxAmount` = `Floor(Reduced8TaxableAmount x 0.08)`
  - 端数処理: **税率区分ごとの合計金額に対して税率を乗じた後、Floor（切り捨て）**
  - ※明細行ごとの消費税額を積み上げる方式とは異なる（インボイス制度対応）

#### 2.1.4 標準税率10%の計算（Standard Rate 10% Calculation）
- `Standard10TaxableAmount` = 標準税率（`TaxRateType.Standard10`）対象明細の税抜小計合計
- `Standard10TaxAmount` = `Floor(Standard10TaxableAmount x 0.10)`

#### 2.1.5 消費税合計・税込合計（Tax Total & Grand Total）
- `TotalTaxAmount` = `Reduced8TaxAmount + Standard10TaxAmount`
- `TotalAmount` = `SubtotalExcludingTax + TotalTaxAmount`

### 2.2 明細行の税計算（CartItem Tax Calculation）
各明細行（`CartItem`）においても個別に税額を保持する（レシート印刷等のため）。
- `CartItem.SubtotalExcludingTax` = `UnitPrice x Quantity`
- `CartItem.TaxAmount` = `Floor(SubtotalExcludingTax x TaxRate)`
- `CartItem.SubtotalIncludingTax` = `SubtotalExcludingTax + TaxAmount`
- ※この行単位の税額は参照用であり、合計税額の算出には使用しない（インボイス制度上、税率区分ごとの集計を優先）

### 2.3 売上サマリー表示パネル（Summary Display Panel）
カートに商品が1点以上存在する場合、左パネル下部に以下のサマリー情報を表示する。

| 表示項目 | 内容 |
|---|---|
| 合計点数 | TotalQuantity 点 |
| 税抜合計 | SubtotalExcludingTax 円 |
| 8%対象 | Reduced8TaxableAmount 円（税額: Reduced8TaxAmount 円） |
| 10%対象 | Standard10TaxableAmount 円（税額: Standard10TaxAmount 円） |
| 消費税合計 | TotalTaxAmount 円 |
| **税込合計** | **TotalAmount 円**（強調表示） |

### 2.4 会計ボタンの制御（Checkout Button State）
- カートに1件以上の商品が存在し、かつ `TotalAmount > 0` の場合にのみ「会計へ」ボタンを有効化。
- 上記条件を満たさない場合はボタンを無効化し、押下時はエラーメッセージを表示。
- `CanOpenAccounting` = `HasCartItems AND TotalAmount > 0`

### 2.5 カートの変更検知と自動更新（Cart Change Detection）
- カートへの商品追加・削除・数量変更のたびに `SaleService.CartChanged` イベントが発火される。
- `MainViewModel` はこのイベントをサブスクライブし、`SaleSummary` の再計算と画面バインディング値の更新を自動で行う。
- 更新対象のプロパティ: `TotalQuantity`, `SubtotalExcludingTax`, `Reduced8TaxableAmount`, `Reduced8TaxAmount`,
  `Standard10TaxableAmount`, `Standard10TaxAmount`, `TotalTaxAmount`, `TotalAmount`, `HasCartItems`, `CanOpenAccounting`

---

## 3. 非機能要件・アーキテクチャ (Architecture & Technical Constraints)

- **設計パターン**: MVVMパターン（`CommunityToolkit.Mvvm`）に厳格に従う。
- **制約**: **コードビハインド禁止（No Code-Behind）**。
- **ドメインモデル**:
  - `SaleSummary`: 不変オブジェクト（コンストラクタで全値を確定）。`CartItem` のリストを受け取り、税計算を実行。
  - `CartItem`: 各明細行のデータを保持するドメインモデル。
- **Service層**: `ISaleService.Summary` プロパティ経由で `SaleSummary` を取得。
- **ViewModel層**: `MainViewModel` が `ISaleService` から `SaleSummary` を取得し、各プロパティにバインディング。
- **単体テスト**: `xUnit` による以下のシナリオをカバーすること。
  - 空カートの `SaleSummary` がすべて0を返すこと
  - 軽減税率のみ・標準税率のみ・混在の各パターンで税額が正確に計算されること
  - 端数（小数点以下）が切り捨て（Floor）されること
  - カート変更イベントによりViewModelのサマリープロパティが更新されること
  - `CanOpenAccounting` がカートの状態に応じて正しく切り替わること
