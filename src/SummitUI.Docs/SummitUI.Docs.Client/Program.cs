using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SummitUI.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddSummitUI();
await builder.Build().RunAsync();