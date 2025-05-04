using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;
using OdooMapping.Domain.Models;

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
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestPostgresConnectionAsync(string connectionString)
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetSqlServerDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand("SELECT name FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }
            
            return databases;
        }

        public async Task<List<string>> GetPostgresDatabasesAsync(string connectionString)
        {
            var databases = new List<string>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datistemplate = false AND datname NOT IN ('postgres')", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                databases.Add(reader.GetString(0));
            }
            
            return databases;
        }

        public async Task<List<string>> GetSqlServerTablesAsync(string connectionString)
        {
            var tables = new List<string>();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
            
            return tables;
        }

        public async Task<List<string>> GetPostgresTablesAsync(string connectionString)
        {
            var tables = new List<string>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new NpgsqlCommand(@"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE'
                ORDER BY table_name", connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
            
            return tables;
        }

        public async Task<List<string>> GetSqlServerColumnsAsync(string connectionString, string tableName)
        {
            var columns = new List<string>();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY ORDINAL_POSITION", connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }
            
            return columns;
        }

        public async Task<List<string>> GetPostgresColumnsAsync(string connectionString, string tableName)
        {
            var columns = new List<string>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            using var command = new NpgsqlCommand(@"
                SELECT column_name 
                FROM information_schema.columns 
                WHERE table_schema = 'public' 
                AND table_name = @TableName
                ORDER BY ordinal_position", connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }
            
            return columns;
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

        public async Task<List<OdooModule>> GetInstalledOdooModulesAsync(string connectionString)
        {
            var modules = new List<OdooModule>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Query to get installed modules from Odoo's ir_module_module table
            using var command = new NpgsqlCommand(@"
                SELECT name, state, latest_version, shortdesc 
                FROM ir_module_module 
                WHERE state = 'installed' 
                ORDER BY name", connection);
            
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    modules.Add(new OdooModule
                    {
                        Name = reader.GetString(0),
                        State = reader.GetString(1),
                        Version = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
            catch
            {
                // If this fails, it's probably not an Odoo database or we don't have access
                // Return empty list without throwing exception
            }
            
            return modules;
        }
        
        public async Task<List<OdooModel>> GetOdooModelsAsync(string connectionString)
        {
            var models = new List<OdooModel>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Query to get models from Odoo's ir_model table
            using var command = new NpgsqlCommand(@"
                SELECT m.model, m.name, m.state, m.transient, m.modules
                FROM ir_model m
                WHERE m.state = 'base'
                ORDER BY m.model", connection);
            
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    models.Add(new OdooModel
                    {
                        Model = reader.GetString(0),
                        Name = reader.GetString(1),
                        State = reader.GetString(2),
                        IsTransient = reader.GetBoolean(3),
                        ModuleInfo = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }
            catch
            {
                // If this fails, it's probably not an Odoo database or we don't have access
                // Return empty list without throwing exception
            }
            
            return models;
        }

        public async Task<List<OdooField>> GetOdooFieldsAsync(string connectionString, string modelName)
        {
            var fields = new List<OdooField>();
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Query to get fields from Odoo's ir_model_fields table
            using var command = new NpgsqlCommand(@"
                SELECT f.name, f.field_description, f.ttype, f.relation, f.required, f.readonly, f.store
                FROM ir_model_fields f
                JOIN ir_model m ON f.model_id = m.id
                WHERE m.model = @ModelName
                ORDER BY f.name", connection);
            command.Parameters.AddWithValue("@ModelName", modelName);
            
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    fields.Add(new OdooField
                    {
                        Name = reader.GetString(0),
                        Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                        FieldType = reader.GetString(2),
                        Relation = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Required = reader.GetBoolean(4),
                        ReadOnly = reader.GetBoolean(5),
                        Stored = reader.GetBoolean(6)
                    });
                }
            }
            catch
            {
                // If this fails, it's probably not an Odoo database or we don't have access
                // Return empty list without throwing exception
            }
            
            return fields;
        }

        public async Task<string> GetTableNameForOdooModelAsync(string connectionString, string modelName)
        {
            // Convert Odoo model name (like 'res.partner') to table name (like 'res_partner')
            string tableName = modelName.Replace(".", "_");
            
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Check if table exists
            using var command = new NpgsqlCommand(@"
                SELECT EXISTS (
                    SELECT 1 
                    FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = @TableName
                )", connection);
            command.Parameters.AddWithValue("@TableName", tableName);
            
            bool exists = (bool)await command.ExecuteScalarAsync();
            
            return exists ? tableName : null;
        }

        public async Task<Dictionary<string, string>> GetOdooFieldMappingForTableAsync(string connectionString, string tableName)
        {
            // This maps database column names to Odoo field names
            var fieldMapping = new Dictionary<string, string>();
            
            // Convert table name to Odoo model name
            string modelName = tableName.Replace("_", ".");
            
            // Get all fields for this model
            var fields = await GetOdooFieldsAsync(connectionString, modelName);
            
            // For each field, determine the column name in the database
            foreach (var field in fields)
            {
                if (field.Stored)
                {
                    string columnName = field.Name;
                    
                    // Special handling for relational fields
                    if (field.FieldType == "many2one" && field.Relation != null)
                    {
                        columnName = field.Name + "_id";
                    }
                    
                    fieldMapping[columnName] = field.Name;
                }
            }
            
            return fieldMapping;
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