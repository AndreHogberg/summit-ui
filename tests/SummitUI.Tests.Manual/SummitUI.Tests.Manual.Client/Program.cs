using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using SummitUI;
using SummitUI.Extensions;
using SummitUI.Tests.Manual.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add SummitUI services
builder.Services.AddSummitUI();
builder.Services.AddToastQueue<TestToastContent>();

await builder.Build().RunAsync();
