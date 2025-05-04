using Microsoft.Extensions.DependencyInjection;
using OdooMapping.Application.Interfaces;
using OdooMapping.Application.Services;

namespace OdooMapping.Application
{
    /// <summary>
    /// Extension methods for setting up application services in DI
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds all application services to the service collection
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IMappingService, MappingService>();
            
            return services;
        }
    }
} 