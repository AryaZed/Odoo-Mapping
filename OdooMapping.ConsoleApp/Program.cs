using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OdooMapping.Application;
using OdooMapping.Application.Interfaces;
using OdooMapping.Domain.Models;
using OdooMapping.Infrastructure;
using OdooMapping.Infrastructure.Data;
using OdooMapping.Infrastructure.Persistence;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OdooMapping.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Odoo Data Mapping Tool");
            Console.WriteLine("======================\n");
            
            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
                
                // Setup DI
                var services = new ServiceCollection();
                ConfigureServices(services, configuration);
                var serviceProvider = services.BuildServiceProvider();
                
                // Run the mapping operation
                if (args.Length > 0 && int.TryParse(args[0], out int mappingId))
                {
                    await RunMapping(serviceProvider, mappingId);
                }
                else
                {
                    ShowUsage();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
            // Add application services
            services.AddApplication();
            
            // Add infrastructure services
            services.AddInfrastructure();
            
            // Add specific data services with named services
            services.AddScoped<IDataAccessService, SqlServerDataService>(provider => 
                new SqlServerDataService());
                
            services.AddScoped<IDataAccessService, PostgreSqlDataService>(provider => 
                new PostgreSqlDataService());
        }
        
        private static async Task RunMapping(ServiceProvider serviceProvider, int mappingId)
        {
            var mappingService = serviceProvider.GetRequiredService<IMappingService>();
            
            Console.WriteLine($"Executing mapping with ID: {mappingId}");
            Console.WriteLine("Processing...");
            
            var result = await mappingService.ExecuteMappingAsync(mappingId);
            
            if (result.IsSuccessful)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Success! Processed {result.RecordsProcessed} records in {result.ExecutionTimeSeconds:F2} seconds.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {result.ErrorMessage}");
                Console.ResetColor();
            }
        }
        
        private static void ShowUsage()
        {
            Console.WriteLine("Usage: OdooMapping.ConsoleApp <mapping-id>");
            Console.WriteLine("\nExample: OdooMapping.ConsoleApp 1");
        }
    }
}
