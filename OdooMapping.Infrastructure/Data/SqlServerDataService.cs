using Microsoft.Data.SqlClient;
using OdooMapping.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OdooMapping.Infrastructure.Data
{
    /// <summary>
    /// SQL Server implementation of data access service
    /// </summary>
    public class SqlServerDataService : IDataAccessService
    {
        /// <summary>
        /// Executes a query against SQL Server and returns the results as a DataTable
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string connectionString, string query)
        {
            var dataTable = new DataTable();
            
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = 300; // 5 minutes timeout
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            
            return dataTable;
        }
        
        /// <summary>
        /// Bulk inserts data into a SQL Server table
        /// </summary>
        public async Task<int> BulkInsertAsync(string connectionString, string tableName, DataTable data, bool truncateFirst = false)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                if (truncateFirst)
                {
                    using (var command = new SqlCommand($"TRUNCATE TABLE {tableName}", connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BatchSize = 1000;
                    bulkCopy.BulkCopyTimeout = 600; // 10 minutes timeout
                    
                    // Map columns
                    foreach (DataColumn column in data.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    
                    await bulkCopy.WriteToServerAsync(data);
                }
            }
            
            return data.Rows.Count;
        }
        
        /// <summary>
        /// Tests if a connection can be established with SQL Server
        /// </summary>
        public async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
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
        /// Gets column information about a SQL Server table
        /// </summary>
        public async Task<IEnumerable<string>> GetTableColumnsAsync(string connectionString, string tableName)
        {
            var columns = new List<string>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = @TableName 
                    ORDER BY ORDINAL_POSITION";
                
                using (var command = new SqlCommand(query, connection))
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
        /// Gets a list of all tables in the SQL Server database
        /// </summary>
        public async Task<IEnumerable<string>> GetTablesAsync(string connectionString)
        {
            var tables = new List<string>();
            
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT TABLE_NAME
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    AND TABLE_NAME NOT LIKE 'sys%'
                    AND TABLE_NAME NOT LIKE 'database_%'
                    ORDER BY TABLE_NAME";
                
                using (var command = new SqlCommand(query, connection))
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
        /// Gets a list of available databases on the SQL Server
        /// </summary>
        public async Task<IEnumerable<string>> GetDatabasesAsync(string serverConnectionString)
        {
            var databases = new List<string>();
            
            // Extract server info from connection string but remove initial catalog/database if present
            var builder = new SqlConnectionStringBuilder(serverConnectionString);
            builder.InitialCatalog = "master"; // Connect to master database to query available databases
            
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                
                string query = @"
                    SELECT name
                    FROM sys.databases
                    WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
                    ORDER BY name";
                
                using (var command = new SqlCommand(query, connection))
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
        /// Gets a preview of data from a SQL Server table
        /// </summary>
        public async Task<DataTable> GetTablePreviewAsync(string connectionString, string tableName, int maxRows = 10)
        {
            var query = $"SELECT TOP {maxRows} * FROM [{tableName}]";
            return await ExecuteQueryAsync(connectionString, query);
        }
    }
} 