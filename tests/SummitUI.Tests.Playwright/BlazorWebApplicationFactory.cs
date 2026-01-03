using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SummitUI.Tests.Playwright;

/// <summary>
/// A WebApplicationFactory configured for Blazor application testing with Playwright.
/// Uses .NET 10's new UseKestrel() and StartServer() APIs to spin up a real Kestrel server.
/// </summary>
public class BlazorWebApplicationFactory : WebApplicationFactory<SummitUI.Tests.Manual.Components.App>
{

    public BlazorWebApplicationFactory()
    {
    }

    public string ServerAddress { get; set; } = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}
