using System;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Defines a mapping between a source field and target field
    /// </summary>
    public class FieldMapping
    {
        public int Id { get; set; }
        public int MappingDefinitionId { get; set; }
        
        // Source field information
        public string SourceField { get; set; }
        public string SourceFieldType { get; set; }
        
        // Target field information
        public string TargetField { get; set; }
        public string TargetFieldType { get; set; }
        
        // Transformation settings
        public bool RequiresTransformation { get; set; }
        public string TransformationExpression { get; set; }
        public string DefaultValue { get; set; }
        
        // Navigation property
        public MappingDefinition MappingDefinition { get; set; }
    }
} 