# DATABASE.md — DB設計

## 使用DB
**SQLite** (`Microsoft.Data.Sqlite`)

## データベースファイル
- **パス**: `%LocalAppData%\ConvenienceStorePOS\pos.db`
- **カスタムパス**: コンストラクタ引数で指定可能（テスト用）
- 初回起動時に自動作成

## テーブル定義

### Products テーブル（商品マスタ）
| カラム | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | INTEGER | PRIMARY KEY AUTOINCREMENT | 商品ID |
| `Code` | TEXT | NOT NULL UNIQUE | JANコード |
| `Name` | TEXT | NOT NULL | 商品名 |
| `Price` | NUMERIC | NOT NULL | 単価（税抜） |
| `TaxRateType` | INTEGER | NOT NULL | 税率区分（8 or 10） |
| `Category` | TEXT | NOT NULL | カテゴリ名 |
| `StockQuantity` | INTEGER | NOT NULL DEFAULT 100 | 在庫数 |
| `IsActive` | INTEGER | NOT NULL DEFAULT 1 | 有効フラグ |

インデックス: `IX_Products_Code`, `IX_Products_Category`

### Sales テーブル（取引ヘッダ）
| カラム | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | INTEGER | PRIMARY KEY AUTOINCREMENT | 取引ID |
| `TransactionNumber` | TEXT | NOT NULL UNIQUE | 取引番号 |
| `CreatedAt` | TEXT | NOT NULL | 取引日時（ISO 8601） |
| `TotalQuantity` | INTEGER | NOT NULL | 合計点数 |
| `SubtotalExcludingTax` | NUMERIC | NOT NULL | 税抜合計 |
| `Reduced8TaxableAmount` | NUMERIC | NOT NULL | 8%対象税抜額 |
| `Reduced8TaxAmount` | NUMERIC | NOT NULL | 8%消費税額 |
| `Standard10TaxableAmount` | NUMERIC | NOT NULL | 10%対象税抜額 |
| `Standard10TaxAmount` | NUMERIC | NOT NULL | 10%消費税額 |
| `TotalTaxAmount` | NUMERIC | NOT NULL | 消費税合計 |
| `TotalAmount` | NUMERIC | NOT NULL | 税込合計金額 |
| `PaymentMethod` | INTEGER | NOT NULL | 支払方法区分 |
| `ReceivedAmount` | NUMERIC | NOT NULL | お預かり金額 |
| `ChangeAmount` | NUMERIC | NOT NULL | お釣り金額 |
| `StaffName` | TEXT | NOT NULL | レジ担当者名 |
| `RegisterNumber` | TEXT | NOT NULL | レジ番号 |

インデックス: `IX_Sales_TransactionNumber`, `IX_Sales_CreatedAt`

### SaleDetails テーブル（取引明細）
| カラム | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | INTEGER | PRIMARY KEY AUTOINCREMENT | 明細ID |
| `SaleId` | INTEGER | NOT NULL, FK(Sales.Id) ON DELETE CASCADE | 取引ヘッダID |
| `ProductId` | INTEGER | NOT NULL | 商品ID |
| `ProductCode` | TEXT | NOT NULL | JANコード |
| `ProductName` | TEXT | NOT NULL | 商品名 |
| `UnitPrice` | NUMERIC | NOT NULL | 単価（税抜） |
| `TaxRateType` | INTEGER | NOT NULL | 税率区分 |
| `Quantity` | INTEGER | NOT NULL | 数量 |

インデックス: `IX_SaleDetails_SaleId`

## シードデータ（25商品、5カテゴリ）

### おにぎり・弁当（軽減税率8%）
| 商品名 | 価格(税抜) | JANコード |
|---|---|---|
| 手巻おにぎり 熟成紅しゃけ | ¥160 | 4901001000018 |
| 手巻おにぎり ツナマヨネーズ | ¥140 | 4901001000025 |
| 具たっぷり 幕の内弁当 | ¥550 | 4901001000032 |
| 特製チキン南蛮弁当 | ¥590 | 4901001000049 |
| ジューシーハムレタスサンド | ¥280 | 4901001000056 |

### 飲料（軽減税率8%）
| 商品名 | 価格(税抜) | JANコード |
|---|---|---|
| 厳選緑茶 500ml | ¥130 | 4901002000015 |
| 香り立つブラックコーヒー 400ml | ¥120 | 4901002000022 |
| 南アルプス天然水 550ml | ¥100 | 4901002000039 |
| 香ばしカフェラテ 240ml | ¥168 | 4901002000046 |
| ビタミンC レモンソーダ 500ml | ¥150 | 4901002000053 |

### ホットスナック（軽減税率8%）
| 商品名 | 価格(税抜) | JANコード |
|---|---|---|
| ジューシープレミアムフライドチキン | ¥213 | 4901003000012 |
| 旨辛からあげ棒 | ¥170 | 4901003000029 |
| ジャンボフランクフルト | ¥165 | 4901003000036 |
| 北海道ポテトコロッケ | ¥100 | 4901003000043 |
| 極旨肉まん | ¥150 | 4901003000050 |

### 菓子・デザート（軽減税率8%）
| 商品名 | 価格(税抜) | JANコード |
|---|---|---|
| なめらか濃厚カスタードプリン | ¥198 | 4901004000019 |
| もちもちロールケーキ | ¥180 | 4901004000026 |
| ポテトチップス うすしお味 | ¥148 | 4901004000033 |
| ミルクチョコレート 50g | ¥130 | 4901004000040 |
| ひとくちチョコシュー | ¥120 | 4901004000057 |

### 日用品（標準税率10%）
| 商品名 | 価格(税抜) | JANコード |
|---|---|---|
| 65cmジャンプ耐風ビニール傘 | ¥650 | 4901005000016 |
| ポケットティッシュ 4個入 | ¥120 | 4901005000023 |
| アルコール除菌ウェットティッシュ | ¥200 | 4901005000030 |
| 急速充電Type-Cケーブル 1m | ¥880 | 4901005000047 |
| 不織布マスク ふつうサイズ 7枚入 | ¥320 | 4901005000054 |
