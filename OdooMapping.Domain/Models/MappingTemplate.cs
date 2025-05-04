using System;
using System.Collections.Generic;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Represents a reusable template for database mappings
    /// </summary>
    public class MappingTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        
        // Source database information
        public string SourceConnectionString { get; set; }
        public string SourceTable { get; set; }
        public List<string> SourceColumns { get; set; } = new List<string>();
        
        // Target database information
        public string TargetConnectionString { get; set; }
        public string TargetTable { get; set; }
        public List<string> TargetColumns { get; set; } = new List<string>();
        
        // Execution settings
        public bool TruncateTargetBeforeInsert { get; set; }
        public int BatchSize { get; set; } = 100;
        
        // Field mappings
        public List<FieldMapping> FieldMappings { get; set; } = new List<FieldMapping>();
        
        // Custom validation rules
        public List<ValidationRule> ValidationRules { get; set; } = new List<ValidationRule>();

        /// <summary>
        /// Creates a new mapping definition from this template
        /// </summary>
        public MappingDefinition CreateMappingDefinition()
        {
            return new MappingDefinition
            {
                Name = Name,
                Description = Description,
                CreatedAt = DateTime.Now,
                SourceConnectionString = SourceConnectionString,
                SourceQuery = $"SELECT {string.Join(", ", SourceColumns)} FROM {SourceTable}",
                TargetConnectionString = TargetConnectionString,
                TargetTable = TargetTable,
                TruncateTargetBeforeInsert = TruncateTargetBeforeInsert,
                BatchSize = BatchSize,
                FieldMappings = new List<FieldMapping>(FieldMappings),
                ValidationRules = new List<ValidationRule>(ValidationRules)
            };
        }
    }

    /// <summary>
    /// Represents a validation rule for data mapping
    /// </summary>
    public class ValidationRule
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Field { get; set; }
        public string RuleType { get; set; }
        public string Expression { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Error;
        public bool IsEnabled { get; set; } = true;
    }

    /// <summary>
    /// Represents the severity level of a validation rule
    /// </summary>
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }
} 