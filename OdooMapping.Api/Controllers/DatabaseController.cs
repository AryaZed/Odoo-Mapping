using Microsoft.AspNetCore.Mvc;
using OdooMapping.Application.Interfaces;
using OdooMapping.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace OdooMapping.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly SqlServerDataService _sqlServerService;
        private readonly PostgreSqlDataService _postgresService;
        
        public DatabaseController(
            SqlServerDataService sqlServerService,
            PostgreSqlDataService postgresService)
        {
            _sqlServerService = sqlServerService;
            _postgresService = postgresService;
        }
        
        // GET: api/database/sqlserver/test
        [HttpGet("sqlserver/test")]
        public async Task<IActionResult> TestSqlServerConnection([FromQuery] string connectionString)
        {
            try
            {
                bool result = await _sqlServerService.TestConnectionAsync(connectionString);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/postgres/test
        [HttpGet("postgres/test")]
        public async Task<IActionResult> TestPostgresConnection([FromQuery] string connectionString)
        {
            try
            {
                bool result = await _postgresService.TestConnectionAsync(connectionString);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/sqlserver/databases
        [HttpGet("sqlserver/databases")]
        public async Task<ActionResult<IEnumerable<string>>> GetSqlServerDatabases([FromQuery] string connectionString)
        {
            try
            {
                var databases = await _sqlServerService.GetDatabasesAsync(connectionString);
                return Ok(databases);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/postgres/databases
        [HttpGet("postgres/databases")]
        public async Task<ActionResult<IEnumerable<string>>> GetPostgresDatabases([FromQuery] string connectionString)
        {
            try
            {
                var databases = await _postgresService.GetDatabasesAsync(connectionString);
                return Ok(databases);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/sqlserver/tables
        [HttpGet("sqlserver/tables")]
        public async Task<ActionResult<IEnumerable<string>>> GetSqlServerTables([FromQuery] string connectionString)
        {
            try
            {
                var tables = await _sqlServerService.GetTablesAsync(connectionString);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/postgres/tables
        [HttpGet("postgres/tables")]
        public async Task<ActionResult<IEnumerable<string>>> GetPostgresTables([FromQuery] string connectionString)
        {
            try
            {
                var tables = await _postgresService.GetTablesAsync(connectionString);
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/sqlserver/columns
        [HttpGet("sqlserver/columns")]
        public async Task<ActionResult<IEnumerable<string>>> GetSqlServerColumns([FromQuery] string connectionString, [FromQuery] string tableName)
        {
            try
            {
                var columns = await _sqlServerService.GetTableColumnsAsync(connectionString, tableName);
                return Ok(columns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/postgres/columns
        [HttpGet("postgres/columns")]
        public async Task<ActionResult<IEnumerable<string>>> GetPostgresColumns([FromQuery] string connectionString, [FromQuery] string tableName)
        {
            try
            {
                var columns = await _postgresService.GetTableColumnsAsync(connectionString, tableName);
                return Ok(columns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/sqlserver/preview
        [HttpGet("sqlserver/preview")]
        public async Task<ActionResult<DataTable>> GetSqlServerPreview([FromQuery] string connectionString, [FromQuery] string tableName, [FromQuery] int maxRows = 10)
        {
            try
            {
                var data = await _sqlServerService.GetTablePreviewAsync(connectionString, tableName, maxRows);
                
                // Convert DataTable to a serializable format
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in data.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in data.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    rows.Add(dict);
                }
                
                return Ok(new
                {
                    columns = GetColumnInfo(data),
                    rows
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        // GET: api/database/postgres/preview
        [HttpGet("postgres/preview")]
        public async Task<ActionResult<DataTable>> GetPostgresPreview([FromQuery] string connectionString, [FromQuery] string tableName, [FromQuery] int maxRows = 10)
        {
            try
            {
                var data = await _postgresService.GetTablePreviewAsync(connectionString, tableName, maxRows);
                
                // Convert DataTable to a serializable format
                var rows = new List<Dictionary<string, object>>();
                foreach (DataRow row in data.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in data.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                    }
                    rows.Add(dict);
                }
                
                return Ok(new
                {
                    columns = GetColumnInfo(data),
                    rows
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        private List<Dictionary<string, object>> GetColumnInfo(DataTable dataTable)
        {
            var columns = new List<Dictionary<string, object>>();
            
            foreach (DataColumn column in dataTable.Columns)
            {
                columns.Add(new Dictionary<string, object>
                {
                    { "name", column.ColumnName },
                    { "type", column.DataType.Name }
                });
            }
            
            return columns;
        }
    }
} 