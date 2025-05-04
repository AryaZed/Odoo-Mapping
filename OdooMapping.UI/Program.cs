using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OdooMapping.UI;
using OdooMapping.UI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Setup HTTP client services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add API client services
builder.Services.AddHttpClient("OdooMappingApi", client => 
{
    client.BaseAddress = new Uri("https://localhost:7241");  // Default API address
});

// Add custom services
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<MappingService>();

await builder.Build().RunAsync();
