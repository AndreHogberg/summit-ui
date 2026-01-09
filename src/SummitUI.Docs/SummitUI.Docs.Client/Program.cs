using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using SummitUI.Docs.Client;
using SummitUI.Docs.Client.Services;
using SummitUI.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddSummitUI();
builder.Services.AddToastQueue<DocsToastContent>();
builder.Services.AddScoped<ThemeJsInterop>();
await builder.Build().RunAsync();
