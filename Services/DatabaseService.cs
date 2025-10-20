using Dapper;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FruitSysWeb.Models.Common;
using System.Text.RegularExpressions;

namespace FruitSysWeb.Services
{
    public class DatabaseService
    {
        private readonly string? _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<T> QuerySingleAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QuerySingleAsync<T>(sql, parameters);
        }

        // DODANA METODA - ExecuteScalarAsync
        public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Koristimo ExecuteScalarAsync sa Dapper
            var result = await connection.ExecuteScalarAsync<T?>(sql, parameters);
            return result ?? default(T)!;
        }

        // DODANA METODA - QuerySingleOrDefaultAsync
        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        // ========================================
        // PAGINATION SUPPORT
        // ========================================

        /// <summary>
        /// Executes a paged query and returns results with pagination metadata.
        /// Automatically adds LIMIT and OFFSET to the query.
        /// </summary>
        /// <typeparam name="T">Type of result items</typeparam>
        /// <param name="sql">SQL query (without LIMIT/OFFSET)</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="pagination">Pagination request (page number and size)</param>
        /// <returns>Paged result with data and metadata</returns>
        public async Task<PagedResult<T>> QueryPagedAsync<T>(
            string sql,
            object? parameters = null,
            PaginationRequest? pagination = null)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            pagination ??= PaginationRequest.Default;

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Build count query by extracting SELECT ... FROM ... WHERE ... ORDER BY
            var countSql = BuildCountQuery(sql);

            // Get total count
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            _logger.LogInformation(
                "Pagination query - Total: {TotalCount}, Page: {PageNumber}/{TotalPages}, PageSize: {PageSize}",
                totalCount,
                pagination.PageNumber,
                pagination.PageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pagination.PageSize) : 0,
                pagination.PageSize);

            // Add LIMIT and OFFSET to original query
            var pagedSql = $"{sql}\nLIMIT @PageSize OFFSET @Offset";

            // Merge parameters with pagination params
            var pagedParameters = MergeParameters(parameters, new
            {
                PageSize = pagination.PageSize,
                Offset = pagination.Offset
            });

            // Execute paged query
            var data = await connection.QueryAsync<T>(pagedSql, pagedParameters);

            return new PagedResult<T>
            {
                Data = data,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }

        /// <summary>
        /// Builds a COUNT query from a SELECT query
        /// </summary>
        private string BuildCountQuery(string sql)
        {
            // Remove ORDER BY clause (not needed for COUNT)
            var sqlWithoutOrderBy = Regex.Replace(sql, @"\s+ORDER\s+BY\s+.+$", "", RegexOptions.IgnoreCase);

            // Check if query already has SELECT COUNT(*)
            if (Regex.IsMatch(sqlWithoutOrderBy, @"SELECT\s+COUNT\s*\(", RegexOptions.IgnoreCase))
            {
                return sqlWithoutOrderBy;
            }

            // Replace SELECT ... with SELECT COUNT(*)
            // Handle SELECT DISTINCT
            if (Regex.IsMatch(sqlWithoutOrderBy, @"SELECT\s+DISTINCT", RegexOptions.IgnoreCase))
            {
                // For DISTINCT queries, wrap in subquery
                return $"SELECT COUNT(*) FROM ({sqlWithoutOrderBy}) AS CountQuery";
            }

            // Regular SELECT - replace with COUNT(*)
            var countSql = Regex.Replace(
                sqlWithoutOrderBy,
                @"SELECT\s+.+?\s+FROM",
                "SELECT COUNT(*) FROM",
                RegexOptions.IgnoreCase);

            return countSql;
        }

        /// <summary>
        /// Merges multiple parameter objects into a single DynamicParameters object
        /// </summary>
        private DynamicParameters MergeParameters(params object?[] parameterObjects)
        {
            var merged = new DynamicParameters();

            foreach (var obj in parameterObjects)
            {
                if (obj != null)
                {
                    merged.AddDynamicParams(obj);
                }
            }

            return merged;
        }

        // ========================================
        // TRANSACTION SUPPORT
        // ========================================

        /// <summary>
        /// Executes multiple database operations within a transaction scope.
        /// Automatically commits on success or rolls back on exception.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="operation">Async operation to execute within transaction</param>
        /// <returns>Result of the operation</returns>
        public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Transaction started");

                var result = await operation(connection, transaction);

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed, rolling back");
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Executes multiple database operations within a transaction scope (void return).
        /// Automatically commits on success or rolls back on exception.
        /// </summary>
        /// <param name="operation">Async operation to execute within transaction</param>
        public async Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operation)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Transaction started");

                await operation(connection, transaction);

                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed, rolling back");
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Creates a new DatabaseTransaction for manual transaction control.
        /// </summary>
        /// <returns>DatabaseTransaction instance</returns>
        public async Task<DatabaseTransaction> BeginTransactionAsync()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();
            _logger.LogInformation("Manual transaction started");

            return new DatabaseTransaction(connection, transaction, _logger);
        }
    }

    /// <summary>
    /// Represents a database transaction with manual control.
    /// Use with 'using' statement to ensure proper disposal.
    /// </summary>
    public class DatabaseTransaction : IAsyncDisposable
    {
        private readonly MySqlConnection _connection;
        private readonly IDbTransaction _transaction;
        private readonly ILogger _logger;
        private bool _committed;
        private bool _rolledBack;

        internal DatabaseTransaction(MySqlConnection connection, IDbTransaction transaction, ILogger logger)
        {
            _connection = connection;
            _transaction = transaction;
            _logger = logger;
        }

        public IDbConnection Connection => _connection;
        public IDbTransaction Transaction => _transaction;

        /// <summary>
        /// Commits the transaction
        /// </summary>
        public async Task CommitAsync()
        {
            if (_committed || _rolledBack)
            {
                throw new InvalidOperationException("Transaction already completed");
            }

            await Task.Run(() => _transaction.Commit());
            _committed = true;
            _logger.LogInformation("Manual transaction committed");
        }

        /// <summary>
        /// Rolls back the transaction
        /// </summary>
        public async Task RollbackAsync()
        {
            if (_committed || _rolledBack)
            {
                throw new InvalidOperationException("Transaction already completed");
            }

            await Task.Run(() => _transaction.Rollback());
            _rolledBack = true;
            _logger.LogWarning("Manual transaction rolled back");
        }

        /// <summary>
        /// Executes a query within this transaction
        /// </summary>
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            return await _connection.QueryAsync<T>(sql, parameters, _transaction);
        }

        /// <summary>
        /// Executes a command within this transaction
        /// </summary>
        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            return await _connection.ExecuteAsync(sql, parameters, _transaction);
        }

        /// <summary>
        /// Executes a scalar query within this transaction
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            var result = await _connection.ExecuteScalarAsync<T?>(sql, parameters, _transaction);
            return result ?? default(T)!;
        }

        public async ValueTask DisposeAsync()
        {
            // Auto-rollback if not committed
            if (!_committed && !_rolledBack)
            {
                _logger.LogWarning("Transaction was not committed, auto-rolling back");
                await RollbackAsync();
            }

            // Dispose transaction synchronously (IDbTransaction doesn't have DisposeAsync)
            _transaction.Dispose();
            await _connection.DisposeAsync();
        }
    }
}
