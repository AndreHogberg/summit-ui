using System.Diagnostics;

namespace SummitUI.Tests.Playwright;

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

        // Wait for the server to be ready by making a health check request
        await WaitForServerReadyAsync(ServerUrl, TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Waits for the server to be ready to accept requests.
    /// This reduces flakiness caused by tests starting before the server is fully initialized.
    /// </summary>
    private static async Task WaitForServerReadyAsync(string serverUrl, TimeSpan timeout)
    {
        using var httpClient = new HttpClient();
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                var response = await httpClient.GetAsync(serverUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Server ready after {stopwatch.ElapsedMilliseconds}ms");
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet, wait and retry
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"Server at {serverUrl} did not become ready within {timeout.TotalSeconds} seconds");
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
