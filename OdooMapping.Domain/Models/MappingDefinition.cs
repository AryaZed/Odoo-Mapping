using System;
using System.Collections.Generic;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Defines a mapping between a source database and Odoo database
    /// </summary>
    public class MappingDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastExecuted { get; set; }
        
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
        
        // Tracking information
        public string LastExecutionLog { get; set; }
        public int LastRecordCount { get; set; }
        public bool LastExecutionSuccessful { get; set; }
    }
} 