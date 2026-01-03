// Here you could define global logic that would affect all tests

// You can use attributes at the assembly level to apply to all tests in the assembly

[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

// Retry flaky tests up to 2 times to reduce flakiness in Playwright tests
[assembly: TUnit.Core.Retry(2)]

// Run ALL Playwright tests sequentially to prevent system overload
// Each Playwright test requires significant resources (browser instance, network, etc.)

// Note: To override via command line, use:
// dotnet run --project ArkUI.Tests.Playwright -- --maximum-parallel-tests 1