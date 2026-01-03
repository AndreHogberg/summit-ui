using ArkUI.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add ArkUI services
builder.Services.AddArkUI();

await builder.Build().RunAsync();