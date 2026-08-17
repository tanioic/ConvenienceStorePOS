using System.Data.Common;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Microsoft.Data.Sqlite;

namespace ConvenienceStorePOS.Data
{
    public class SqliteProductRepository : IProductRepository
    {
        private readonly IDatabaseInitializer _databaseInitializer;

        public SqliteProductRepository(IDatabaseInitializer databaseInitializer)
        {
            _databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));
        }

        private async Task<SqliteConnection> GetOpenConnectionAsync()
        {
            var connection = new SqliteConnection(_databaseInitializer.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT Id, Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive FROM Products WHERE Code = @Code AND IsActive = 1 LIMIT 1";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Code", code.Trim());

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT Id, Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive FROM Products WHERE Id = @Id LIMIT 1";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync()
        {
            var list = new List<Product>();
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT Id, Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive FROM Products WHERE IsActive = 1 ORDER BY Category, Id";
            using var command = new SqliteCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Product>> GetByCategoryAsync(string category)
        {
            var list = new List<Product>();
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT Id, Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive FROM Products WHERE Category = @Category AND IsActive = 1 ORDER BY Id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Category", category);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<Product>> SearchAsync(string keyword, string? category = null)
        {
            var list = new List<Product>();
            using var connection = await GetOpenConnectionAsync();

            var sql = "SELECT Id, Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive FROM Products WHERE IsActive = 1";
            if (!string.IsNullOrWhiteSpace(category) && category != "全て" && category != "All")
            {
                sql += " AND Category = @Category";
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sql += " AND (Name LIKE @Keyword OR Code LIKE @Keyword)";
            }
            sql += " ORDER BY Category, Id";

            using var command = new SqliteCommand(sql, connection);
            if (!string.IsNullOrWhiteSpace(category) && category != "全て" && category != "All")
            {
                command.Parameters.AddWithValue("@Category", category);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                command.Parameters.AddWithValue("@Keyword", $"%{keyword.Trim()}%");
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapProduct(reader));
            }

            return list;
        }

        public async Task<IReadOnlyList<string>> GetCategoriesAsync()
        {
            var categories = new List<string>();
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT DISTINCT Category FROM Products WHERE IsActive = 1 ORDER BY Category";
            using var command = new SqliteCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(reader.GetString(0));
            }

            return categories;
        }

        public async Task AddAsync(Product product)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                INSERT INTO Products (Code, Name, Price, TaxRateType, Category, StockQuantity, IsActive)
                VALUES (@Code, @Name, @Price, @TaxRateType, @Category, @StockQuantity, @IsActive);
                SELECT last_insert_rowid();
            ";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Code", product.Code);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@TaxRateType", (int)product.TaxRateType);
            command.Parameters.AddWithValue("@Category", product.Category);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@IsActive", product.IsActive ? 1 : 0);

            var id = await command.ExecuteScalarAsync();
            product.Id = Convert.ToInt32(id);
        }

        public async Task UpdateAsync(Product product)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                UPDATE Products
                SET Code = @Code,
                    Name = @Name,
                    Price = @Price,
                    TaxRateType = @TaxRateType,
                    Category = @Category,
                    StockQuantity = @StockQuantity,
                    IsActive = @IsActive
                WHERE Id = @Id;
            ";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", product.Id);
            command.Parameters.AddWithValue("@Code", product.Code);
            command.Parameters.AddWithValue("@Name", product.Name);
            command.Parameters.AddWithValue("@Price", product.Price);
            command.Parameters.AddWithValue("@TaxRateType", (int)product.TaxRateType);
            command.Parameters.AddWithValue("@Category", product.Category);
            command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
            command.Parameters.AddWithValue("@IsActive", product.IsActive ? 1 : 0);

            await command.ExecuteNonQueryAsync();
        }

        private static Product MapProduct(DbDataReader reader)
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Code = reader.GetString(1),
                Name = reader.GetString(2),
                Price = reader.GetDecimal(3),
                TaxRateType = (TaxRateType)reader.GetInt32(4),
                Category = reader.GetString(5),
                StockQuantity = reader.GetInt32(6),
                IsActive = reader.GetInt32(7) == 1
            };
        }
    }
}
