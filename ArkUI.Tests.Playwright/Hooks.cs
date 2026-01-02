using System.Diagnostics;

namespace ArkUI.Tests.Playwright;

public class Hooks
{
    private static BlazorWebApplicationFactory? _factory;

    /// <summary>
    /// Gets the base URL of the running Blazor application server.
    /// </summary>
    public static string ServerUrl { get; private set; } = string.Empty;

    [Before(TestSession)]
    public static async Task SetupTestSession()
    {
        if (Debugger.IsAttached)
        {
            Environment.SetEnvironmentVariable("PWDEBUG", "1");
        }

        // Install Playwright browsers
        Microsoft.Playwright.Program.Main(["install"]);

        // Start the Blazor application server using .NET 10 WebApplicationFactory
        _factory = new BlazorWebApplicationFactory();
        _factory.UseKestrel();
        _factory.StartServer();
        _factory.ServerAddress = _factory.ClientOptions.BaseAddress.ToString() ?? string.Empty;
        ServerUrl = _factory.ServerAddress;
        Console.WriteLine($"Blazor server started at: {_factory.ServerAddress}");
    }

    [After(TestSession)]
    public static async Task TeardownTestSession()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
            _factory = null;
            Console.WriteLine("Blazor server stopped.");
        }
    }
}