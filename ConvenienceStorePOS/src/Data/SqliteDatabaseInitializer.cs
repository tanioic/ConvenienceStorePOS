using System.IO;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Microsoft.Data.Sqlite;

namespace ConvenienceStorePOS.Data
{
    public class SqliteDatabaseInitializer : IDatabaseInitializer
    {
        private readonly string _dbPath;

        public string ConnectionString => $"Data Source={_dbPath}";

        public SqliteDatabaseInitializer(string? dbPath = null)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ConvenienceStorePOS");

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }

                _dbPath = Path.Combine(appDataFolder, "pos.db");
            }
            else
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                _dbPath = dbPath;
            }
        }

        public async Task InitializeAsync()
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var createTableSql = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Code TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    Price NUMERIC NOT NULL,
                    TaxRateType INTEGER NOT NULL,
                    Category TEXT NOT NULL,
                    StockQuantity INTEGER NOT NULL DEFAULT 100,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );
                CREATE INDEX IF NOT EXISTS IX_Products_Code ON Products(Code);
                CREATE INDEX IF NOT EXISTS IX_Products_Category ON Products(Category);

                CREATE TABLE IF NOT EXISTS Sales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransactionNumber TEXT NOT NULL UNIQUE,
                    CreatedAt TEXT NOT NULL,
                    TotalQuantity INTEGER NOT NULL,
                    SubtotalExcludingTax NUMERIC NOT NULL,
                    Reduced8TaxableAmount NUMERIC NOT NULL,
                    Reduced8TaxAmount NUMERIC NOT NULL,
                    Standard10TaxableAmount NUMERIC NOT NULL,
                    Standard10TaxAmount NUMERIC NOT NULL,
                    TotalTaxAmount NUMERIC NOT NULL,
                    TotalAmount NUMERIC NOT NULL,
                    PaymentMethod INTEGER NOT NULL,
                    ReceivedAmount NUMERIC NOT NULL,
                    ChangeAmount NUMERIC NOT NULL,
                    StaffName TEXT NOT NULL,
                    RegisterNumber TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Sales_TransactionNumber ON Sales(TransactionNumber);
                CREATE INDEX IF NOT EXISTS IX_Sales_CreatedAt ON Sales(CreatedAt);

                CREATE TABLE IF NOT EXISTS SaleDetails (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductCode TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    UnitPrice NUMERIC NOT NULL,
                    TaxRateType INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    FOREIGN KEY(SaleId) REFERENCES Sales(Id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS IX_SaleDetails_SaleId ON SaleDetails(SaleId);
            ";

            using (var command = new SqliteCommand(createTableSql, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Check if seeding is necessary
            var countSql = "SELECT COUNT(*) FROM Products";
            using (var countCommand = new SqliteCommand(countSql, connection))
            {
                var count = Convert.ToInt64(await countCommand.ExecuteScalarAsync());
                if (count == 0)
                {
                    await SeedProductsAsync(connection);
                }
            }
        }

        private async Task SeedProductsAsync(SqliteConnection connection)
        {
            var seedProducts = new List<Product>
            {
                // おにぎり・弁当 (軽減税率 8%)
                new() { Code = "4901001000018", Name = "手巻おにぎり 熟成紅しゃけ", Price = 160m, TaxRateType = TaxRateType.Reduced8, Category = "おにぎり・弁当" },
                new() { Code = "4901001000025", Name = "手巻おにぎり ツナマヨネーズ", Price = 140m, TaxRateType = TaxRateType.Reduced8, Category = "おにぎり・弁当" },
                new() { Code = "4901001000032", Name = "具たっぷり 幕の内弁当", Price = 550m, TaxRateType = TaxRateType.Reduced8, Category = "おにぎり・弁当" },
                new() { Code = "4901001000049", Name = "特製チキン南蛮弁当", Price = 590m, TaxRateType = TaxRateType.Reduced8, Category = "おにぎり・弁当" },
                new() { Code = "4901001000056", Name = "ジューシーハムレタスサンド", Price = 280m, TaxRateType = TaxRateType.Reduced8, Category = "おにぎり・弁当" },

                // 飲料 (軽減税率 8%)
                new() { Code = "4901002000015", Name = "厳選緑茶 500ml", Price = 130m, TaxRateType = TaxRateType.Reduced8, Category = "飲料" },
                new() { Code = "4901002000022", Name = "香り立つブラックコーヒー 400ml", Price = 120m, TaxRateType = TaxRateType.Reduced8, Category = "飲料" },
                new() { Code = "4901002000039", Name = "南アルプス天然水 550ml", Price = 100m, TaxRateType = TaxRateType.Reduced8, Category = "飲料" },
                new() { Code = "4901002000046", Name = "香ばしカフェラテ 240ml", Price = 168m, TaxRateType = TaxRateType.Reduced8, Category = "飲料" },
                new() { Code = "4901002000053", Name = "ビタミンC レモンソーダ 500ml", Price = 150m, TaxRateType = TaxRateType.Reduced8, Category = "飲料" },

                // ホットスナック (軽減税率 8%)
                new() { Code = "4901003000012", Name = "ジューシープレミアムフライドチキン", Price = 213m, TaxRateType = TaxRateType.Reduced8, Category = "ホットスナック" },
                new() { Code = "4901003000029", Name = "旨辛からあげ棒", Price = 170m, TaxRateType = TaxRateType.Reduced8, Category = "ホットスナック" },
                new() { Code = "4901003000036", Name = "ジャンボフランクフルト", Price = 165m, TaxRateType = TaxRateType.Reduced8, Category = "ホットスナック" },
                new() { Code = "4901003000043", Name = "北海道ポテトコロッケ", Price = 100m, TaxRateType = TaxRateType.Reduced8, Category = "ホットスナック" },
                new() { Code = "4901003000050", Name = "極旨肉まん", Price = 150m, TaxRateType = TaxRateType.Reduced8, Category = "ホットスナック" },

                // 菓子・デザート (軽減税率 8%)
                new() { Code = "4901004000019", Name = "なめらか濃厚カスタードプリン", Price = 198m, TaxRateType = TaxRateType.Reduced8, Category = "菓子・デザート" },
                new() { Code = "4901004000026", Name = "もちもちロールケーキ", Price = 180m, TaxRateType = TaxRateType.Reduced8, Category = "菓子・デザート" },
                new() { Code = "4901004000033", Name = "ポテトチップス うすしお味", Price = 148m, TaxRateType = TaxRateType.Reduced8, Category = "菓子・デザート" },
                new() { Code = "4901004000040", Name = "ミルクチョコレート 50g", Price = 130m, TaxRateType = TaxRateType.Reduced8, Category = "菓子・デザート" },
                new() { Code = "4901004000057", Name = "ひとくちチョコシュー", Price = 120m, TaxRateType = TaxRateType.Reduced8, Category = "菓子・デザート" },

                // 日用品 (標準税率 10%)
                new() { Code = "4901005000016", Name = "65cmジャンプ耐風ビニール傘", Price = 650m, TaxRateType = TaxRateType.Standard10, Category = "日用品" },
                new() { Code = "4901005000023", Name = "ポケットティッシュ 4個入", Price = 120m, TaxRateType = TaxRateType.Standard10, Category = "日用品" },
                new() { Code = "4901005000030", Name = "アルコール除菌ウェットティッシュ", Price = 200m, TaxRateType = TaxRateType.Standard10, Category = "日用品" },
                new() { Code = "4901005000047", Name = "急速充電Type-Cケーブル 1m", Price = 880m, TaxRateType = TaxRateType.Standard10, Category = "日用品" },
                new() { Code = "4901005000054", Name = "不織布マスク ふつうサイズ 7枚入", Price = 320m, TaxRateType = TaxRateType.Standard10, Category = "日用品" }
            };

            using var transaction = connection.BeginTransaction();
            var insertSql = @"
                INSERT INTO Products (Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive)
                VALUES (@Code, @Name, @Price, @TaxRateType, @Category, @StockQuantity, @IsActive);
            ";

            foreach (var p in seedProducts)
            {
                using var command = new SqliteCommand(insertSql, connection, transaction);
                command.Parameters.AddWithValue("@Code", p.Code);
                command.Parameters.AddWithValue("@Name", p.Name);
                command.Parameters.AddWithValue("@Price", p.Price);
                command.Parameters.AddWithValue("@TaxRateType", (int)p.TaxRateType);
                command.Parameters.AddWithValue("@Category", p.Category);
                command.Parameters.AddWithValue("@StockQuantity", p.StockQuantity);
                command.Parameters.AddWithValue("@IsActive", p.IsActive ? 1 : 0);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
    }
}
