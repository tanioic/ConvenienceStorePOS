# SPEC-003: 会計・決済処理（Accounting）仕様書

## 1. 概要 (Overview)
コンビニエンスストアPOSシステムにおける「会計（決済）処理」の仕様を定義する。
取引明細（カート）に登録された商品群に対して、支払方法（現金、クレジットカード、電子マネー、QR・バーコード決済）を選択し、お預かり金額の入力・お釣りの算出を行い、売上トランザクションデータをSQLiteデータベースへ永続化して取引を完了する。

---

## 2. 業務要件 (Functional Requirements)

### 2.1 支払方法の選択 (Payment Methods)
- **現金 (Cash)**:
  - お預かり金額を入力し、お釣り（釣銭）を自動算出。
  - テンキー（0〜9, 00, クリア）およびクイック金種ボタン（「ちょうど」「1,000円」「5,000円」「10,000円」「+1,000円」等）による高速入力。
  - お預かり金額が税込合計金額未満の場合は決済確定不可（不足額を警告表示）。
- **クレジットカード (Credit Card)**:
  - 全額カード決済（お預かり金額＝合計金額、お釣り＝0円）。
  - 承認シミュレーション（承認番号等の生成）。
- **電子マネー (Electronic Money)**:
  - 交通系IC, iD, QUICPay等による全額決済。
- **QR・バーコード決済 (QR / Barcode Pay)**:
  - PayPay, 楽天ペイ, d払い, au PAY等による全額決済。

### 2.2 お預かり・お釣り計算 (Change Calculation)
- **お釣り計算式**: `お釣り = お預かり金額 - 税込合計金額`
- **金種内訳算出**: 10,000円札、5,000円札、1,000円札、500円玉、100円玉、50円玉、10円玉、5円玉、1円玉の枚数を自動算出。

### 2.3 売上トランザクションの永続化 (Database Persistence)
- 会計確定時に、以下の2つのテーブルへトランザクション保存を行う：
  1. `Sales` テーブル（取引ヘッダ）:
     - `Id`: 主キー
     - `TransactionNumber`: 取引番号（例: `TRX-20260817-XXXX`）
     - `CreatedAt`: 取引日時
     - `TotalQuantity`: 合計点数
     - `SubtotalExcludingTax`: 税抜合計
     - `Reduced8TaxableAmount`: 8%対象税抜額
     - `Reduced8TaxAmount`: 8%消費税額
     - `Standard10TaxableAmount`: 10%対象税抜額
     - `Standard10TaxAmount`: 10%消費税額
     - `TotalTaxAmount`: 消費税合計
     - `TotalAmount`: 税込合計金額
     - `PaymentMethod`: 支払方法区分
     - `ReceivedAmount`: お預かり金額
     - `ChangeAmount`: お釣り金額
     - `StaffName`: レジ担当者名
     - `RegisterNumber`: レジ番号
  2. `SaleDetails` テーブル（取引明細）:
     - `Id`: 主キー
     - `SaleId`: 取引ヘッダID（外部キー）
     - `ProductId`: 商品ID
     - `ProductCode`: 商品JANコード
     - `ProductName`: 商品名
     - `UnitPrice`: 単価（税抜）
     - `TaxRateType`: 税率区分（8% / 10%）
     - `TaxRate`: 税率小数値
     - `Quantity`: 数量
     - `SubtotalExcludingTax`: 税抜小計
     - `TaxAmount`: 消費税額
     - `SubtotalIncludingTax`: 税込小計

### 2.4 会計完了と次客リセット (Completion & Reset)
- 会計確定後、完了画面（支払方法、お預かり、お釣り、取引番号）を表示。
- 「次の取引へ」ボタン押下により、カートおよび会計状態を即座にリセットし、初期待機状態へ遷移。

---

## 3. 非機能要件・アーキテクチャ (Architecture & Constraints)
- **MVVMパターン**: `CommunityToolkit.Mvvm` を全面活用。
- **制約**: **コードビハインド禁止（No Code-Behind）**。
- **Repositoryパターン**: `ISaleRepository` / `SqliteSaleRepository` による売上履歴・明細の保存。
- **Service層**: `IAccountingService` / `AccountingService` による会計ビジネスロジック。
- **依存性注入 (DI)**: `App.xaml.cs` にて各サービスを登録。
- **単体テスト**: `xUnit` による網羅的テスト。
