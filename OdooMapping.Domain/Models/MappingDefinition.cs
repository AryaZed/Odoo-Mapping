using System;
using System.Collections.Generic;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Defines a mapping between a source database and Odoo database
    /// </summary>
    public class MappingDefinition
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastExecutedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Source configuration
        public string SourceConnectionString { get; set; }
        public string SourceQuery { get; set; }
        
        // Target configuration
        public string TargetConnectionString { get; set; }
        public string TargetTable { get; set; }
        
        // Execution settings
        public bool TruncateTargetBeforeInsert { get; set; }
        public int BatchSize { get; set; } = 100;
        
        // Collection of field mappings
        public List<FieldMapping> FieldMappings { get; set; } = new List<FieldMapping>();
        
        // Validation rules
        public List<ValidationRule> ValidationRules { get; set; } = new List<ValidationRule>();
        
        // Scheduling information
        public bool IsScheduled { get; set; }
        public string Schedule { get; set; }
        public DateTime? NextScheduledRun { get; set; }
        
        // Tracking information
        public string LastExecutionLog { get; set; }
        public int LastRecordCount { get; set; }
        public bool LastExecutionSuccessful { get; set; }
    }
}