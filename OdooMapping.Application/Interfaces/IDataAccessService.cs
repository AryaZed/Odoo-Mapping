using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OdooMapping.Application.Interfaces
{
    /// <summary>
    /// Interface for database access operations
    /// </summary>
    public interface IDataAccessService
    {
        /// <summary>
        /// Executes a query and returns the results as a DataTable
        /// </summary>
        Task<DataTable> ExecuteQueryAsync(string connectionString, string query);
        
        /// <summary>
        /// Bulk inserts data into a target table
        /// </summary>
        Task<int> BulkInsertAsync(string connectionString, string tableName, DataTable data, bool truncateFirst = false);
        
        /// <summary>
        /// Tests if a connection can be established with the provided connection string
        /// </summary>
        Task<bool> TestConnectionAsync(string connectionString);
        
        /// <summary>
        /// Gets schema information about a database table
        /// </summary>
        Task<IEnumerable<string>> GetTableColumnsAsync(string connectionString, string tableName);
        
        /// <summary>
        /// Gets a list of all tables in the database
        /// </summary>
        Task<IEnumerable<string>> GetTablesAsync(string connectionString);
        
        /// <summary>
        /// Gets a list of available databases on the server
        /// </summary>
        Task<IEnumerable<string>> GetDatabasesAsync(string serverConnectionString);
        
        /// <summary>
        /// Gets a preview of data from a table
        /// </summary>
        Task<DataTable> GetTablePreviewAsync(string connectionString, string tableName, int maxRows = 10);
    }
} 