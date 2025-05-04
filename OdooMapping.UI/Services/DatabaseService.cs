using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OdooMapping.UI.Services
{
    public class DatabaseService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public DatabaseService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient ApiClient => _clientFactory.CreateClient("OdooMappingApi");

        public async Task<bool> TestSqlServerConnectionAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/sqlserver/test?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
            return result.success;
        }

        public async Task<bool> TestPostgresConnectionAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/postgres/test?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
            return result.success;
        }

        public async Task<List<string>> GetSqlServerDatabasesAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/sqlserver/databases?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<List<string>> GetPostgresDatabasesAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/postgres/databases?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<List<string>> GetSqlServerTablesAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/sqlserver/tables?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<List<string>> GetPostgresTablesAsync(string connectionString)
        {
            var response = await ApiClient.GetAsync($"api/database/postgres/tables?connectionString={Uri.EscapeDataString(connectionString)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<List<string>> GetSqlServerColumnsAsync(string connectionString, string tableName)
        {
            var response = await ApiClient.GetAsync($"api/database/sqlserver/columns?connectionString={Uri.EscapeDataString(connectionString)}&tableName={Uri.EscapeDataString(tableName)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<List<string>> GetPostgresColumnsAsync(string connectionString, string tableName)
        {
            var response = await ApiClient.GetAsync($"api/database/postgres/columns?connectionString={Uri.EscapeDataString(connectionString)}&tableName={Uri.EscapeDataString(tableName)}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<List<string>>(_jsonOptions);
        }

        public async Task<TablePreviewResponse> GetSqlServerPreviewAsync(string connectionString, string tableName, int maxRows = 10)
        {
            var response = await ApiClient.GetAsync($"api/database/sqlserver/preview?connectionString={Uri.EscapeDataString(connectionString)}&tableName={Uri.EscapeDataString(tableName)}&maxRows={maxRows}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TablePreviewResponse>(_jsonOptions);
        }

        public async Task<TablePreviewResponse> GetPostgresPreviewAsync(string connectionString, string tableName, int maxRows = 10)
        {
            var response = await ApiClient.GetAsync($"api/database/postgres/preview?connectionString={Uri.EscapeDataString(connectionString)}&tableName={Uri.EscapeDataString(tableName)}&maxRows={maxRows}");
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<TablePreviewResponse>(_jsonOptions);
        }
    }

    public class TablePreviewResponse
    {
        public List<ColumnInfo> Columns { get; set; }
        public List<Dictionary<string, object>> Rows { get; set; }
    }

    public class ColumnInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
} 