using OdooMapping.Domain.Models;
using System.Threading.Tasks;

namespace OdooMapping.Application.Interfaces
{
    /// <summary>
    /// Interface for data mapping operations between databases
    /// </summary>
    public interface IMappingService
    {
        /// <summary>
        /// Executes a mapping operation from source to target database
        /// </summary>
        Task<MappingResult> ExecuteMappingAsync(int mappingId);
        
        /// <summary>
        /// Executes a mapping operation from source to target database
        /// </summary>
        Task<MappingResult> ExecuteMappingAsync(MappingDefinition mapping);
        
        /// <summary>
        /// Validates a mapping definition for errors or inconsistencies
        /// </summary>
        Task<ValidationResult> ValidateMappingAsync(int mappingId);
        
        /// <summary>
        /// Validates a mapping definition for errors or inconsistencies
        /// </summary>
        Task<ValidationResult> ValidateMappingAsync(MappingDefinition mapping);
    }
    
    /// <summary>
    /// Represents the result of a mapping operation
    /// </summary>
    public class MappingResult
    {
        public bool IsSuccessful { get; set; }
        public int RecordsProcessed { get; set; }
        public string ErrorMessage { get; set; }
        public double ExecutionTimeSeconds { get; set; }
    }
    
    /// <summary>
    /// Represents the result of a mapping validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string[] ValidationErrors { get; set; }
    }
} 