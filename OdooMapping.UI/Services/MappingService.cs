using OdooMapping.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OdooMapping.UI.Services
{
    public class MappingService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;

        public MappingService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient ApiClient => _clientFactory.CreateClient("OdooMappingApi");

        public async Task<List<MappingDefinition>> GetAllMappingsAsync()
        {
            var response = await ApiClient.GetAsync("api/mappings");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MappingDefinition>>(_jsonOptions);
        }

        public async Task<MappingDefinition> GetMappingAsync(Guid id)
        {
            var response = await ApiClient.GetAsync($"api/mappings/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MappingDefinition>(_jsonOptions);
        }

        public async Task<MappingDefinition> CreateMappingAsync(MappingDefinition mapping)
        {
            var response = await ApiClient.PostAsJsonAsync("api/mappings", mapping);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MappingDefinition>(_jsonOptions);
        }

        public async Task UpdateMappingAsync(Guid id, MappingDefinition mapping)
        {
            var response = await ApiClient.PutAsJsonAsync($"api/mappings/{id}", mapping);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateMappingAsync(MappingDefinition mapping)
        {
            await UpdateMappingAsync(mapping.Id, mapping);
        }

        public async Task DeleteMappingAsync(Guid id)
        {
            var response = await ApiClient.DeleteAsync($"api/mappings/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<ValidationResult> ValidateMappingAsync(Guid id)
        {
            var response = await ApiClient.PostAsync($"api/mappings/{id}/validate", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ValidationResult>(_jsonOptions);
            }
            else
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ValidationResult>(_jsonOptions);
                return errorResponse;
            }
        }

        public async Task<MappingResult> ExecuteMappingAsync(Guid id)
        {
            var response = await ApiClient.PostAsync($"api/mappings/{id}/execute", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<MappingResult>(_jsonOptions);
            }
            else
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<MappingResult>(_jsonOptions);
                return errorResponse;
            }
        }

        public async Task<List<MappingTemplate>> GetMappingTemplatesAsync()
        {
            var response = await ApiClient.GetAsync("api/templates");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MappingTemplate>>(_jsonOptions);
        }

        public async Task<MappingTemplate> GetMappingTemplateAsync(Guid id)
        {
            var response = await ApiClient.GetAsync($"api/templates/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MappingTemplate>(_jsonOptions);
        }

        public async Task<MappingTemplate> CreateMappingTemplateAsync(MappingTemplate template)
        {
            var response = await ApiClient.PostAsJsonAsync("api/templates", template);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MappingTemplate>(_jsonOptions);
        }

        public async Task UpdateMappingTemplateAsync(Guid id, MappingTemplate template)
        {
            var response = await ApiClient.PutAsJsonAsync($"api/templates/{id}", template);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMappingTemplateAsync(Guid id)
        {
            var response = await ApiClient.DeleteAsync($"api/templates/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<MappingExecution>> GetMappingExecutionsAsync(Guid mappingId)
        {
            var response = await ApiClient.GetAsync($"api/mappings/{mappingId}/executions");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MappingExecution>>(_jsonOptions);
        }
        
        public async Task<List<MappingExecution>> GetMappingExecutionsAsync()
        {
            var response = await ApiClient.GetAsync("api/executions");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MappingExecution>>(_jsonOptions);
        }

        public async Task<List<MappingDefinition>> GetMappingsAsync()
        {
            return await GetAllMappingsAsync();
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class MappingResult
    {
        public bool Success { get; set; }
        public int RowsProcessed { get; set; }
        public int RowsInserted { get; set; }
        public int RowsFailed { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Logs { get; set; } = new List<string>();
    }
} 