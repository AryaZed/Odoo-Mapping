using Npgsql;
using OdooMapping.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OdooMapping.Infrastructure.Data
{
    /// <summary>
    /// PostgreSQL implementation of data access service for Odoo
    /// </summary>
    public class PostgreSqlDataService : IDataAccessService
    {
        /// <summary>
        /// Executes a query against PostgreSQL and returns the results as a DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string connectionString, string query)
        {
            var dataTable = new DataTable();
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.CommandTimeout = 300; // 5 minutes timeout
                    
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            
            return dataTable;
        }
        
        /// <summary>
        /// Bulk inserts data into a PostgreSQL table
        /// </summary>
        public async Task<int> BulkInsertAsync(string connectionString, string tableName, DataTable data, bool truncateFirst = false)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                if (truncateFirst)
                {
                    using (var command = new NpgsqlCommand($"TRUNCATE TABLE {tableName} RESTART IDENTITY CASCADE", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                int rowsInserted = 0;
                
                // Create parameterized SQL for insertion
                var columnNames = new List<string>();
                var paramNames = new List<string>();
                
                foreach (DataColumn column in data.Columns)
                {
                    columnNames.Add($"\"{column.ColumnName}\"");
                    paramNames.Add($"@{column.ColumnName}");
                }
                
                string insertSql = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";
                
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        using (var command = new NpgsqlCommand(insertSql, connection, (NpgsqlTransaction)transaction))
                        {
                            // Create parameters
                            foreach (DataColumn column in data.Columns)
                            {
                                var parameter = command.CreateParameter();
                                parameter.ParameterName = $"@{column.ColumnName}";
                                parameter.NpgsqlDbType = MapDotNetTypeToNpgsqlType(column.DataType);
                                command.Parameters.Add(parameter);
                            }
                            
                            // Execute batches of inserts
                            foreach (DataRow row in data.Rows)
                            {
                                for (int i = 0; i < data.Columns.Count; i++)
                                {
                                    command.Parameters[i].Value = row[i] == DBNull.Value ? DBNull.Value : row[i];
                                }
                                
                                await command.ExecuteNonQueryAsync();
                                rowsInserted++;
                            }
                            
                            await transaction.CommitAsync();
                        }
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                
                return rowsInserted;
            }
        }
        
        /// <summary>
        /// Tests if a connection can be established with PostgreSQL
        /// </summary>
        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Gets column information about a PostgreSQL table
        /// </summary>
        public async Task<IEnumerable<string>> GetTableColumnsAsync(string connectionString, string tableName)
        {
            var columns = new List<string>();
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT column_name 
                    FROM information_schema.columns 
                    WHERE table_name = @TableName 
                    ORDER BY ordinal_position";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            columns.Add(reader.GetString(0));
                        }
                    }
                }
            }
            
            return columns;
        }
        
        /// <summary>
        /// Gets a list of all tables in the PostgreSQL database
        /// </summary>
        public async Task<IEnumerable<string>> GetTablesAsync(string connectionString)
        {
            var tables = new List<string>();
            
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                // For Odoo, we want to filter tables that are part of the application
                // Odoo tables generally start with specific prefixes
                string query = @"
                    SELECT table_name
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                    AND table_type = 'BASE TABLE'
                    AND table_name NOT LIKE 'pg_%'
                    AND table_name NOT LIKE 'sql_%'
                    ORDER BY table_name";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tables.Add(reader.GetString(0));
                        }
                    }
                }
            }
            
            return tables;
        }
        
        /// <summary>
        /// Gets a list of available databases on the PostgreSQL server
        /// </summary>
        public async Task<IEnumerable<string>> GetDatabasesAsync(string serverConnectionString)
        {
            var databases = new List<string>();
            
            // Modify the connection string to connect to the default 'postgres' database
            var builder = new NpgsqlConnectionStringBuilder(serverConnectionString);
            string originalDatabase = builder.Database;
            builder.Database = "postgres";
            
            using (var connection = new NpgsqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT datname
                    FROM pg_database
                    WHERE datistemplate = false
                    AND datname != 'postgres'
                    ORDER BY datname";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            databases.Add(reader.GetString(0));
                        }
                    }
                }
            }
            
            return databases;
        }
        
        /// <summary>
        /// Gets a preview of data from a PostgreSQL table
        /// </summary>
        public async Task<DataTable> GetTablePreviewAsync(string connectionString, string tableName, int maxRows = 10)
        {
            var query = $"SELECT * FROM \"{tableName}\" LIMIT {maxRows}";
            return await ExecuteQueryAsync(connectionString, query);
        }
        
        /// <summary>
        /// Maps .NET data types to NpgsqlDbType
        /// </summary>
        private NpgsqlTypes.NpgsqlDbType MapDotNetTypeToNpgsqlType(Type type)
        {
            if (type == typeof(int) || type == typeof(Int32))
                return NpgsqlTypes.NpgsqlDbType.Integer;
            else if (type == typeof(long) || type == typeof(Int64))
                return NpgsqlTypes.NpgsqlDbType.Bigint;
            else if (type == typeof(string))
                return NpgsqlTypes.NpgsqlDbType.Text;
            else if (type == typeof(DateTime))
                return NpgsqlTypes.NpgsqlDbType.Timestamp;
            else if (type == typeof(bool))
                return NpgsqlTypes.NpgsqlDbType.Boolean;
            else if (type == typeof(decimal))
                return NpgsqlTypes.NpgsqlDbType.Numeric;
            else if (type == typeof(double))
                return NpgsqlTypes.NpgsqlDbType.Double;
            else if (type == typeof(float))
                return NpgsqlTypes.NpgsqlDbType.Real;
            else
                return NpgsqlTypes.NpgsqlDbType.Unknown;
        }
    }
} 