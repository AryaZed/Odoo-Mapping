using System;
using System.Collections.Generic;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Represents a single execution of a mapping
    /// </summary>
    public class MappingExecution
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid MappingId { get; set; }
        public string MappingName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int TotalRecords { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSkipped { get; set; }
        public int RecordsFailed { get; set; }
        public List<ValidationIssue> ValidationIssues { get; set; } = new List<ValidationIssue>();
        public string ExecutionLog { get; set; }
    }

    /// <summary>
    /// Represents a validation issue encountered during mapping execution
    /// </summary>
    public class ValidationIssue
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ExecutionId { get; set; }
        public Guid RuleId { get; set; }
        public string RuleName { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public string Message { get; set; }
        public ValidationSeverity Severity { get; set; }
        public int RecordIndex { get; set; }
    }
} 