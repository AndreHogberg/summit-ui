using SummitUI.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add SummitUI services
builder.Services.AddSummitUI();

await builder.Build().RunAsync();