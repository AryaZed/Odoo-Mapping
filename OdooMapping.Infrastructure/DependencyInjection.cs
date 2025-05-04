using Microsoft.Extensions.DependencyInjection;
using OdooMapping.Application.Interfaces;
using OdooMapping.Infrastructure.Data;
using OdooMapping.Infrastructure.Repositories;

namespace OdooMapping.Infrastructure
{
    /// <summary>
    /// Extension methods for setting up infrastructure services in DI
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds all infrastructure services to the service collection
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IMappingDefinitionRepository, MappingDefinitionRepository>();
            
            // Register data access services
            services.AddScoped<SqlServerDataService>();
            services.AddScoped<PostgreSqlDataService>();
            
            // Register data service providers - with correct factory pattern that will be resolved based on the database type
            services.AddTransient<IDataAccessService>(serviceProvider => 
            {
                // Default to SQL Server if no specific implementation is requested
                return serviceProvider.GetRequiredService<SqlServerDataService>();
            });
            
            services.AddTransient<IDataAccessService>(serviceProvider => 
            {
                // PostgreSQL for Odoo
                return serviceProvider.GetRequiredService<PostgreSqlDataService>();
            });
            
            return services;
        }
    }
} 