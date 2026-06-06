using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TurnIt;
using TurnIt.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<StorageService>();

await builder.Build().RunAsync();
var app = builder.Build();

// Initialize storage on startup
var storage = app.Services.GetRequiredService<StorageService>();
await storage.Initialize();

await app.RunAsync();
