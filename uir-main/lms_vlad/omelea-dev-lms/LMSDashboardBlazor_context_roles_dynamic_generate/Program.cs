using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LMSDashboardBlazor;
using LMSDashboardBlazor.Services;
using LMSDashboardBlazor.Services.Forms;
using LMSDashboardBlazor.Services.Statements;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddSingleton<AuthOptions>();
builder.Services.AddSingleton<LmsAccessContextService>();
builder.Services.AddScoped<FormSchemaService>();
builder.Services.AddScoped<StatementService>();

await builder.Build().RunAsync();
