using Microsoft.EntityFrameworkCore;
using OdooMapping.Application.Interfaces;
using OdooMapping.Domain.Models;
using OdooMapping.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OdooMapping.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for MappingDefinition entity
    /// </summary>
    public class MappingDefinitionRepository : IMappingDefinitionRepository
    {
        private readonly ApplicationDbContext _context;
        
        public MappingDefinitionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<MappingDefinition> GetByIdAsync(Guid id)
        {
            return await _context.MappingDefinitions.FindAsync(id);
        }
        
        public async Task<IEnumerable<MappingDefinition>> GetAllAsync()
        {
            return await _context.MappingDefinitions.ToListAsync();
        }
        
        public async Task<IEnumerable<MappingDefinition>> FindAsync(Expression<Func<MappingDefinition, bool>> predicate)
        {
            return await _context.MappingDefinitions.Where(predicate).ToListAsync();
        }
        
        public async Task<MappingDefinition> AddAsync(MappingDefinition entity)
        {
            entity.CreatedAt = DateTime.Now;
            _context.MappingDefinitions.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task<MappingDefinition> UpdateAsync(MappingDefinition entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }
        
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.MappingDefinitions.FindAsync(id);
            if (entity == null)
                return false;
                
            _context.MappingDefinitions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public async Task<IEnumerable<MappingDefinition>> GetActiveMappingsAsync()
        {
            return await _context.MappingDefinitions
                .Where(m => m.IsActive)
                .ToListAsync();
        }
        
        public async Task<MappingDefinition> GetMappingWithFieldsAsync(Guid id)
        {
            return await _context.MappingDefinitions
                .Include(m => m.FieldMappings)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
} 