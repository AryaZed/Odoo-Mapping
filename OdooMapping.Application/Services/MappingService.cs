using OdooMapping.Application.Interfaces;
using OdooMapping.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OdooMapping.Application.Services
{
    /// <summary>
    /// Service for handling data mapping operations between SQL Server and Odoo PostgreSQL
    /// </summary>
    public class MappingService : IMappingService
    {
        private readonly IMappingDefinitionRepository _mappingRepository;
        private readonly IDataAccessService _sqlServerDataService;
        private readonly IDataAccessService _postgresDataService;
        
        public MappingService(
            IMappingDefinitionRepository mappingRepository,
            IDataAccessService sqlServerDataService,
            IDataAccessService postgresDataService)
        {
            _mappingRepository = mappingRepository;
            _sqlServerDataService = sqlServerDataService;
            _postgresDataService = postgresDataService;
        }
        
        /// <summary>
        /// Executes a mapping operation by ID
        /// </summary>
        public async Task<MappingResult> ExecuteMappingAsync(int mappingId)
        {
            var mapping = await _mappingRepository.GetMappingWithFieldsAsync(mappingId);
            if (mapping == null)
            {
                return new MappingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = $"Mapping with ID {mappingId} not found."
                };
            }
            
            return await ExecuteMappingAsync(mapping);
        }
        
        /// <summary>
        /// Executes a mapping operation using the provided mapping definition
        /// </summary>
        public async Task<MappingResult> ExecuteMappingAsync(MappingDefinition mapping)
        {
            var result = new MappingResult();
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Validate the mapping first
                var validationResult = await ValidateMappingAsync(mapping);
                if (!validationResult.IsValid)
                {
                    return new MappingResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "Mapping validation failed: " + string.Join(", ", validationResult.ValidationErrors)
                    };
                }
                
                // Extract data from source
                var sourceData = await _sqlServerDataService.ExecuteQueryAsync(
                    mapping.SourceConnectionString,
                    mapping.SourceQuery
                );
                
                // Apply transformations if necessary
                DataTable transformedData = TransformData(sourceData, mapping.FieldMappings);
                
                // Insert into target
                int recordsProcessed = await _postgresDataService.BulkInsertAsync(
                    mapping.TargetConnectionString,
                    mapping.TargetTable,
                    transformedData,
                    mapping.TruncateTargetBeforeInsert
                );
                
                stopwatch.Stop();
                
                // Update mapping with execution details
                mapping.LastExecuted = DateTime.Now;
                mapping.LastRecordCount = recordsProcessed;
                mapping.LastExecutionSuccessful = true;
                mapping.LastExecutionLog = $"Successfully processed {recordsProcessed} records in {stopwatch.Elapsed.TotalSeconds:F2} seconds.";
                
                await _mappingRepository.UpdateAsync(mapping);
                
                // Return success result
                return new MappingResult
                {
                    IsSuccessful = true,
                    RecordsProcessed = recordsProcessed,
                    ExecutionTimeSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Update mapping with failure details
                mapping.LastExecuted = DateTime.Now;
                mapping.LastExecutionSuccessful = false;
                mapping.LastExecutionLog = $"Error: {ex.Message}";
                
                await _mappingRepository.UpdateAsync(mapping);
                
                // Return failure result
                return new MappingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = ex.Message,
                    ExecutionTimeSeconds = stopwatch.Elapsed.TotalSeconds
                };
            }
        }
        
        /// <summary>
        /// Validates a mapping definition by ID
        /// </summary>
        public async Task<ValidationResult> ValidateMappingAsync(int mappingId)
        {
            var mapping = await _mappingRepository.GetMappingWithFieldsAsync(mappingId);
            if (mapping == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    ValidationErrors = new[] { $"Mapping with ID {mappingId} not found." }
                };
            }
            
            return await ValidateMappingAsync(mapping);
        }
        
        /// <summary>
        /// Validates a mapping definition for errors or inconsistencies
        /// </summary>
        public async Task<ValidationResult> ValidateMappingAsync(MappingDefinition mapping)
        {
            var errors = new List<string>();
            
            // Check basic properties
            if (string.IsNullOrWhiteSpace(mapping.Name))
                errors.Add("Mapping name is required.");
                
            if (string.IsNullOrWhiteSpace(mapping.SourceConnectionString))
                errors.Add("Source connection string is required.");
                
            if (string.IsNullOrWhiteSpace(mapping.SourceQuery))
                errors.Add("Source query is required.");
                
            if (string.IsNullOrWhiteSpace(mapping.TargetConnectionString))
                errors.Add("Target connection string is required.");
                
            if (string.IsNullOrWhiteSpace(mapping.TargetTable))
                errors.Add("Target table is required.");
                
            if (mapping.FieldMappings == null || mapping.FieldMappings.Count == 0)
                errors.Add("At least one field mapping is required.");
                
            // Check connections if basic properties are valid
            if (errors.Count == 0)
            {
                // Test source connection
                if (!await _sqlServerDataService.TestConnectionAsync(mapping.SourceConnectionString))
                    errors.Add("Cannot connect to source database with the provided connection string.");
                    
                // Test target connection
                if (!await _postgresDataService.TestConnectionAsync(mapping.TargetConnectionString))
                    errors.Add("Cannot connect to target Odoo database with the provided connection string.");
                    
                // Check target table columns if connections are valid
                if (errors.Count == 0)
                {
                    try
                    {
                        var targetColumns = await _postgresDataService.GetTableColumnsAsync(
                            mapping.TargetConnectionString,
                            mapping.TargetTable
                        );
                        
                        var targetColumnsList = new HashSet<string>(targetColumns, StringComparer.OrdinalIgnoreCase);
                        
                        foreach (var fieldMapping in mapping.FieldMappings)
                        {
                            if (!targetColumnsList.Contains(fieldMapping.TargetField))
                                errors.Add($"Target column '{fieldMapping.TargetField}' does not exist in table '{mapping.TargetTable}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error validating target table columns: {ex.Message}");
                    }
                }
            }
            
            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                ValidationErrors = errors.ToArray()
            };
        }
        
        /// <summary>
        /// Transforms data based on the field mappings
        /// </summary>
        private DataTable TransformData(DataTable sourceData, List<FieldMapping> fieldMappings)
        {
            var resultTable = new DataTable();
            
            // Create columns for the target table
            foreach (var fieldMapping in fieldMappings)
            {
                Type columnType = GetTypeFromString(fieldMapping.TargetFieldType);
                resultTable.Columns.Add(fieldMapping.TargetField, columnType);
            }
            
            // Process each row in the source data
            foreach (DataRow sourceRow in sourceData.Rows)
            {
                var targetRow = resultTable.NewRow();
                
                foreach (var fieldMapping in fieldMappings)
                {
                    object value = null;
                    
                    if (fieldMapping.RequiresTransformation && !string.IsNullOrWhiteSpace(fieldMapping.TransformationExpression))
                    {
                        // Apply the transformation - this is a simplified approach
                        // A real implementation might use a scripting engine or expression evaluator
                        value = ApplyTransformation(sourceRow, fieldMapping);
                    }
                    else if (sourceData.Columns.Contains(fieldMapping.SourceField))
                    {
                        // Direct mapping
                        value = sourceRow[fieldMapping.SourceField];
                    }
                    else
                    {
                        // Source field not found, use default value if provided
                        value = string.IsNullOrWhiteSpace(fieldMapping.DefaultValue)
                            ? DBNull.Value
                            : Convert.ChangeType(fieldMapping.DefaultValue, resultTable.Columns[fieldMapping.TargetField].DataType);
                    }
                    
                    // Handle NULL values
                    targetRow[fieldMapping.TargetField] = value ?? DBNull.Value;
                }
                
                resultTable.Rows.Add(targetRow);
            }
            
            return resultTable;
        }
        
        /// <summary>
        /// Applies a transformation to the source data based on the field mapping
        /// </summary>
        private object ApplyTransformation(DataRow sourceRow, FieldMapping fieldMapping)
        {
            // This is a simple implementation. For a real application, you might:
            // - Use a scripting engine (like Roslyn)
            // - Use a rule engine
            // - Support more complex transformations
            
            string expression = fieldMapping.TransformationExpression.ToLowerInvariant();
            
            if (expression == "uppercase" && sourceRow.Table.Columns.Contains(fieldMapping.SourceField))
            {
                var sourceValue = sourceRow[fieldMapping.SourceField];
                return sourceValue is string ? ((string)sourceValue).ToUpperInvariant() : sourceValue;
            }
            else if (expression == "lowercase" && sourceRow.Table.Columns.Contains(fieldMapping.SourceField))
            {
                var sourceValue = sourceRow[fieldMapping.SourceField];
                return sourceValue is string ? ((string)sourceValue).ToLowerInvariant() : sourceValue;
            }
            else if (expression.StartsWith("concat:") && expression.Length > 7)
            {
                string[] fields = expression.Substring(7).Split(',');
                string result = "";
                
                foreach (var field in fields)
                {
                    string trimmedField = field.Trim();
                    if (sourceRow.Table.Columns.Contains(trimmedField))
                    {
                        result += sourceRow[trimmedField]?.ToString() ?? "";
                    }
                    else
                    {
                        // Treat as literal value if not a field name
                        result += trimmedField;
                    }
                }
                
                return result;
            }
            
            // If no transformation applies or source field doesn't exist, fall back to default value
            return string.IsNullOrWhiteSpace(fieldMapping.DefaultValue)
                ? DBNull.Value
                : fieldMapping.DefaultValue;
        }
        
        /// <summary>
        /// Converts a string type name to a .NET Type
        /// </summary>
        private Type GetTypeFromString(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return typeof(string);
                
            switch (typeName.ToLowerInvariant())
            {
                case "int":
                case "integer":
                    return typeof(int);
                case "bigint":
                case "long":
                    return typeof(long);
                case "decimal":
                case "numeric":
                    return typeof(decimal);
                case "float":
                case "real":
                    return typeof(float);
                case "double":
                    return typeof(double);
                case "boolean":
                case "bool":
                    return typeof(bool);
                case "datetime":
                case "timestamp":
                    return typeof(DateTime);
                case "guid":
                case "uuid":
                    return typeof(Guid);
                default:
                    return typeof(string);
            }
        }
    }
} 