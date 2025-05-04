using Microsoft.AspNetCore.Mvc;
using OdooMapping.Application.Interfaces;
using OdooMapping.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OdooMapping.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MappingsController : ControllerBase
    {
        private readonly IMappingDefinitionRepository _repository;
        private readonly IMappingService _mappingService;
        
        public MappingsController(
            IMappingDefinitionRepository repository,
            IMappingService mappingService)
        {
            _repository = repository;
            _mappingService = mappingService;
        }
        
        // GET: api/mappings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MappingDefinition>>> GetMappings()
        {
            return Ok(await _repository.GetAllAsync());
        }
        
        // GET: api/mappings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MappingDefinition>> GetMapping(Guid id)
        {
            var mapping = await _repository.GetMappingWithFieldsAsync(id);
            
            if (mapping == null)
                return NotFound();
                
            return Ok(mapping);
        }
        
        // POST: api/mappings
        [HttpPost]
        public async Task<ActionResult<MappingDefinition>> CreateMapping(MappingDefinition mapping)
        {
            var result = await _repository.AddAsync(mapping);
            return CreatedAtAction(nameof(GetMapping), new { id = result.Id }, result);
        }
        
        // PUT: api/mappings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapping(Guid id, MappingDefinition mapping)
        {
            if (id != mapping.Id)
                return BadRequest();
                
            var existingMapping = await _repository.GetByIdAsync(id);
            if (existingMapping == null)
                return NotFound();
                
            await _repository.UpdateAsync(mapping);
            return NoContent();
        }
        
        // DELETE: api/mappings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMapping(Guid id)
        {
            var result = await _repository.DeleteAsync(id);
            
            if (!result)
                return NotFound();
                
            return NoContent();
        }
        
        // POST: api/mappings/5/execute
        [HttpPost("{id}/execute")]
        public async Task<IActionResult> ExecuteMapping(Guid id)
        {
            var result = await _mappingService.ExecuteMappingAsync(id);
            
            if (!result.IsSuccessful)
                return BadRequest(result);
                
            return Ok(result);
        }
        
        // POST: api/mappings/5/validate
        [HttpPost("{id}/validate")]
        public async Task<IActionResult> ValidateMapping(Guid id)
        {
            var result = await _mappingService.ValidateMappingAsync(id);
            
            if (!result.IsValid)
                return BadRequest(result);
                
            return Ok(result);
        }
    }
} 