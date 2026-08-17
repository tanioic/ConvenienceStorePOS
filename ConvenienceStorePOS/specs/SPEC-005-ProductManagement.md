# SPEC-005: 商品管理（CRUD）

## 概要
コンビニPOSシステムにおける商品の追加・編集・削除機能を提供する。

## 範囲
- 商品の一覧表示、検索、フィルタ
- 商品の新規追加
- 商品情報の編集
- 商品の削除（論理削除）

## 要件

### 5.1 商品一覧
- 商品一覧をDataGridに表示する
- カテゴリでフィルタできる
- キーワードで検索できる
- 選択した商品を表示する

### 5.2 商品追加
- JANコード（必須、重複不可）
- 商品名（必須）
- 単価（税抜、0以上）
- 消費税区分（標準10% / 軽減8%）
- カテゴリ（必須）
- 在庫数

### 5.3 商品編集
- 選択した商品の情報を編集する
- 編集中は右パネルにフォームを表示する
- 保存時にバリデーションを行う

### 5.4 商品削除
- 選択した商品を論理削除する（IsActive = false）
- 削除前に確認メッセージを表示する

## 画面構成
- 左側: 商品一覧DataGrid + 検索・フィルタバー
- 右側: 操作ボタン / 編集フォーム（トグル表示）
- ヘッダー: 「商品管理」ボタン（メイン画面からアクセス）

## 実装ファイル
- `src/Data/IProductRepository.cs` - DeleteAsync(int id) 追加
- `src/Data/SqliteProductRepository.cs` - DeleteAsync実装（論理削除）
- `src/Services/IProductService.cs` - AddProductAsync, UpdateProductAsync, DeleteProductAsync 追加
- `src/Services/ProductService.cs` - 各メソッド実装（バリデーション付き）
- `src/ViewModels/ProductManagementViewModel.cs` - CRUD操作ViewModel
- `ProductManagementWindow.xaml` - 商品管理画面
- `ProductManagementWindow.xaml.cs` - コードビハインド
- `src/ViewModels/MainViewModel.cs` - OpenProductManagementCommand 追加
- `App.xaml.cs` - DI登録

## テスト
- `Services/ProductServiceSpec005Tests.cs` - バリデーション・リポジトリ委譲テスト
- `ViewModels/ProductManagementViewModelSpec005Tests.cs` - ViewModel動作テスト

## 依存
- SPEC-001（商品データベース）
- SPEC-002（会計処理）
