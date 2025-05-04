using OdooMapping.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OdooMapping.Application.Interfaces
{
    /// <summary>
    /// Repository interface for MappingDefinition entity
    /// </summary>
    public interface IMappingDefinitionRepository : IGuidRepository<MappingDefinition>
    {
        Task<IEnumerable<MappingDefinition>> GetActiveMappingsAsync();
        Task<MappingDefinition> GetMappingWithFieldsAsync(Guid id);
    }
} 