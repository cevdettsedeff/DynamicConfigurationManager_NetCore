using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ConfigurationReader.AdminPanel;
using ConfigurationReader.AdminPanel.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HTTP Client - API base URL
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5000") 
});

// Services
builder.Services.AddScoped<ConfigurationApiService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();