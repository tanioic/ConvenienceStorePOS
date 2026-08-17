using System.Data.Common;
using System.Globalization;
using ConvenienceStorePOS.Common;
using ConvenienceStorePOS.Models;
using Microsoft.Data.Sqlite;

namespace ConvenienceStorePOS.Data
{
    public class SqliteSaleRepository : ISaleRepository
    {
        private readonly IDatabaseInitializer _databaseInitializer;

        public SqliteSaleRepository(IDatabaseInitializer databaseInitializer)
        {
            _databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));
        }

        private async Task<SqliteConnection> GetOpenConnectionAsync()
        {
            var connection = new SqliteConnection(_databaseInitializer.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<SaleTransaction> SaveSaleAsync(SaleTransaction sale, IEnumerable<SaleDetail> details)
        {
            using var connection = await GetOpenConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (string.IsNullOrWhiteSpace(sale.TransactionNumber))
                {
                    sale.TransactionNumber = SaleTransaction.GenerateTransactionNumber();
                }

                var insertSaleSql = @"
                    INSERT INTO Sales (
                        TransactionNumber, CreatedAt, TotalQuantity, SubtotalExcludingTax,
                        Reduced8TaxableAmount, Reduced8TaxAmount, Standard10TaxableAmount, Standard10TaxAmount,
                        TotalTaxAmount, TotalAmount, PaymentMethod, ReceivedAmount, ChangeAmount, StaffName, RegisterNumber
                    ) VALUES (
                        @TransactionNumber, @CreatedAt, @TotalQuantity, @SubtotalExcludingTax,
                        @Reduced8TaxableAmount, @Reduced8TaxAmount, @Standard10TaxableAmount, @Standard10TaxAmount,
                        @TotalTaxAmount, @TotalAmount, @PaymentMethod, @ReceivedAmount, @ChangeAmount, @StaffName, @RegisterNumber
                    );
                    SELECT last_insert_rowid();
                ";

                using (var cmd = new SqliteCommand(insertSaleSql, connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@TransactionNumber", sale.TransactionNumber);
                    cmd.Parameters.AddWithValue("@CreatedAt", sale.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
                    cmd.Parameters.AddWithValue("@TotalQuantity", sale.TotalQuantity);
                    cmd.Parameters.AddWithValue("@SubtotalExcludingTax", sale.SubtotalExcludingTax);
                    cmd.Parameters.AddWithValue("@Reduced8TaxableAmount", sale.Reduced8TaxableAmount);
                    cmd.Parameters.AddWithValue("@Reduced8TaxAmount", sale.Reduced8TaxAmount);
                    cmd.Parameters.AddWithValue("@Standard10TaxableAmount", sale.Standard10TaxableAmount);
                    cmd.Parameters.AddWithValue("@Standard10TaxAmount", sale.Standard10TaxAmount);
                    cmd.Parameters.AddWithValue("@TotalTaxAmount", sale.TotalTaxAmount);
                    cmd.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                    cmd.Parameters.AddWithValue("@PaymentMethod", (int)sale.PaymentMethod);
                    cmd.Parameters.AddWithValue("@ReceivedAmount", sale.ReceivedAmount);
                    cmd.Parameters.AddWithValue("@ChangeAmount", sale.ChangeAmount);
                    cmd.Parameters.AddWithValue("@StaffName", sale.StaffName);
                    cmd.Parameters.AddWithValue("@RegisterNumber", sale.RegisterNumber);

                    var id = await cmd.ExecuteScalarAsync();
                    sale.Id = Convert.ToInt32(id);
                }

                var insertDetailSql = @"
                    INSERT INTO SaleDetails (
                        SaleId, ProductId, ProductCode, ProductName, UnitPrice, TaxRateType, Quantity
                    ) VALUES (
                        @SaleId, @ProductId, @ProductCode, @ProductName, @UnitPrice, @TaxRateType, @Quantity
                    );
                ";

                var detailList = details.ToList();
                foreach (var detail in detailList)
                {
                    detail.SaleId = sale.Id;
                    using var detailCmd = new SqliteCommand(insertDetailSql, connection, transaction);
                    detailCmd.Parameters.AddWithValue("@SaleId", detail.SaleId);
                    detailCmd.Parameters.AddWithValue("@ProductId", detail.ProductId);
                    detailCmd.Parameters.AddWithValue("@ProductCode", detail.ProductCode);
                    detailCmd.Parameters.AddWithValue("@ProductName", detail.ProductName);
                    detailCmd.Parameters.AddWithValue("@UnitPrice", detail.UnitPrice);
                    detailCmd.Parameters.AddWithValue("@TaxRateType", (int)detail.TaxRateType);
                    detailCmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    await detailCmd.ExecuteNonQueryAsync();
                }

                sale.Details = detailList;
                await transaction.CommitAsync();
                return sale;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SaleTransaction?> GetByIdAsync(int id)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT * FROM Sales WHERE Id = @Id LIMIT 1";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var sale = MapSale(reader);
                sale.Details = await LoadDetailsForSaleAsync(sale.Id, connection);
                return sale;
            }

            return null;
        }

        public async Task<SaleTransaction?> GetByTransactionNumberAsync(string transactionNumber)
        {
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT * FROM Sales WHERE TransactionNumber = @TransactionNumber LIMIT 1";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@TransactionNumber", transactionNumber);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var sale = MapSale(reader);
                sale.Details = await LoadDetailsForSaleAsync(sale.Id, connection);
                return sale;
            }

            return null;
        }

        public async Task<IReadOnlyList<SaleTransaction>> GetRecentSalesAsync(int count = 50)
        {
            var sales = new List<SaleTransaction>();
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT * FROM Sales ORDER BY Id DESC LIMIT @Count";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@Count", count);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sales.Add(MapSale(reader));
            }

            return sales;
        }

        private async Task<List<SaleDetail>> LoadDetailsForSaleAsync(int saleId, SqliteConnection connection)
        {
            var details = new List<SaleDetail>();
            var sql = "SELECT Id, SaleId, ProductId, ProductCode, ProductName, UnitPrice, TaxRateType, Quantity FROM SaleDetails WHERE SaleId = @SaleId ORDER BY Id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@SaleId", saleId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                details.Add(new SaleDetail
                {
                    Id = reader.GetInt32(0),
                    SaleId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    ProductCode = reader.GetString(3),
                    ProductName = reader.GetString(4),
                    UnitPrice = reader.GetDecimal(5),
                    TaxRateType = (TaxRateType)reader.GetInt32(6),
                    Quantity = reader.GetInt32(7)
                });
            }

            return details;
        }

        private static SaleTransaction MapSale(DbDataReader reader)
        {
            return new SaleTransaction
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TransactionNumber = reader.GetString(reader.GetOrdinal("TransactionNumber")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")), null, DateTimeStyles.RoundtripKind),
                TotalQuantity = reader.GetInt32(reader.GetOrdinal("TotalQuantity")),
                SubtotalExcludingTax = reader.GetDecimal(reader.GetOrdinal("SubtotalExcludingTax")),
                Reduced8TaxableAmount = reader.GetDecimal(reader.GetOrdinal("Reduced8TaxableAmount")),
                Reduced8TaxAmount = reader.GetDecimal(reader.GetOrdinal("Reduced8TaxAmount")),
                Standard10TaxableAmount = reader.GetDecimal(reader.GetOrdinal("Standard10TaxableAmount")),
                Standard10TaxAmount = reader.GetDecimal(reader.GetOrdinal("Standard10TaxAmount")),
                TotalTaxAmount = reader.GetDecimal(reader.GetOrdinal("TotalTaxAmount")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                PaymentMethod = (PaymentMethod)reader.GetInt32(reader.GetOrdinal("PaymentMethod")),
                ReceivedAmount = reader.GetDecimal(reader.GetOrdinal("ReceivedAmount")),
                ChangeAmount = reader.GetDecimal(reader.GetOrdinal("ChangeAmount")),
                StaffName = reader.GetString(reader.GetOrdinal("StaffName")),
                RegisterNumber = reader.GetString(reader.GetOrdinal("RegisterNumber"))
            };
        }

        public async Task<IReadOnlyList<SaleTransaction>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = new List<SaleTransaction>();
            using var connection = await GetOpenConnectionAsync();
            var sql = "SELECT * FROM Sales WHERE CreatedAt >= @StartDate AND CreatedAt < @EndDate ORDER BY Id DESC";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("o", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("o", CultureInfo.InvariantCulture));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                sales.Add(MapSale(reader));
            }

            return sales;
        }

        public async Task<IReadOnlyList<DailySalesSummary>> GetDailySalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var summaries = new List<DailySalesSummary>();
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                SELECT
                    date(CreatedAt) as SaleDate,
                    COUNT(*) as TransactionCount,
                    SUM(TotalQuantity) as TotalQuantity,
                    SUM(TotalAmount) as TotalAmount,
                    SUM(TotalTaxAmount) as TotalTax,
                    SUM(CASE WHEN PaymentMethod = 1 THEN TotalAmount ELSE 0 END) as CashAmount,
                    SUM(CASE WHEN PaymentMethod != 1 THEN TotalAmount ELSE 0 END) as CashlessAmount
                FROM Sales
                WHERE CreatedAt >= @StartDate AND CreatedAt < @EndDate
                GROUP BY date(CreatedAt)
                ORDER BY SaleDate DESC";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("o", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("o", CultureInfo.InvariantCulture));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                summaries.Add(new DailySalesSummary
                {
                    Date = DateTime.Parse(reader.GetString(0)),
                    TransactionCount = reader.GetInt32(1),
                    TotalQuantity = reader.GetInt32(2),
                    TotalAmount = reader.GetDecimal(3),
                    TotalTax = reader.GetDecimal(4),
                    CashAmount = reader.GetDecimal(5),
                    CashlessAmount = reader.GetDecimal(6)
                });
            }

            return summaries;
        }

        public async Task<IReadOnlyList<CategorySalesSummary>> GetCategorySalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var summaries = new List<CategorySalesSummary>();
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                SELECT
                    sd.ProductName,
                    sd.TaxRateType,
                    SUM(sd.Quantity) as TotalQuantity,
                    SUM(sd.UnitPrice * sd.Quantity) as TotalAmountExcludingTax
                FROM SaleDetails sd
                INNER JOIN Sales s ON sd.SaleId = s.Id
                WHERE s.CreatedAt >= @StartDate AND s.CreatedAt < @EndDate
                GROUP BY sd.ProductName, sd.TaxRateType
                ORDER BY TotalAmountExcludingTax DESC";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("o", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("o", CultureInfo.InvariantCulture));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var taxRateType = (TaxRateType)reader.GetInt32(1);
                var totalExcl = reader.GetDecimal(3);
                var taxRate = taxRateType == TaxRateType.Reduced8 ? 0.08m : 0.10m;
                var tax = Math.Floor(totalExcl * taxRate);

                summaries.Add(new CategorySalesSummary
                {
                    Category = reader.GetString(0),
                    TotalQuantity = reader.GetInt32(2),
                    TotalAmountExcludingTax = totalExcl,
                    TotalTax = tax,
                    TotalAmountIncludingTax = totalExcl + tax
                });
            }

            return summaries;
        }

        public async Task<IReadOnlyList<PaymentMethodSalesSummary>> GetPaymentMethodSalesSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var summaries = new List<PaymentMethodSalesSummary>();
            using var connection = await GetOpenConnectionAsync();
            var sql = @"
                SELECT
                    PaymentMethod,
                    COUNT(*) as TransactionCount,
                    SUM(TotalAmount) as TotalAmount
                FROM Sales
                WHERE CreatedAt >= @StartDate AND CreatedAt < @EndDate
                GROUP BY PaymentMethod
                ORDER BY TotalAmount DESC";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@StartDate", startDate.ToString("o", CultureInfo.InvariantCulture));
            command.Parameters.AddWithValue("@EndDate", endDate.ToString("o", CultureInfo.InvariantCulture));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var pm = (PaymentMethod)reader.GetInt32(0);
                summaries.Add(new PaymentMethodSalesSummary
                {
                    PaymentMethod = (int)pm,
                    PaymentMethodLabel = pm.GetDisplayLabel(),
                    TransactionCount = reader.GetInt32(1),
                    TotalAmount = reader.GetDecimal(2)
                });
            }

            return summaries;
        }
    }
}
