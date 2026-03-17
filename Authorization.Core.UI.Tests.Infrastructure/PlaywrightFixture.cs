using Microsoft.Playwright;

namespace Authorization.Core.UI.Tests.Infrastructure;

/// <summary>
/// Fixture for Playwright browser automation testing.
/// Sets up and tears down browser instances for integration tests.
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright? Playwright { get; private set; }

    public IBrowser? Browser { get; private set; }

    public IBrowserContext? Context { get; private set; }


    /// <summary>
    /// Initializes Playwright and creates a browser instance with virtual authenticator support.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Create Playwright instance
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Launch browser (Chromium for WebAuthn support)
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true, // Set to false for debugging
            SlowMo = 0 // Add delay for debugging if needed
        });

        // Create browser context with permissions
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true, // For self-signed certificates in dev
            Permissions = [] // Add permissions as needed
        });

        // Note: Virtual authenticator support should be configured per-test as needed
        // Playwright's CDP session approach: await page.Context.NewCDPSessionAsync(page)
    }

    /// <summary>
    /// Creates a new page in the current browser context.
    /// </summary>
    public async Task<IPage> CreatePageAsync()
    {
        if (Context == null)
            throw new InvalidOperationException("Browser context not initialized. Call InitializeAsync first.");

        return await Context.NewPageAsync();
    }

    /// <summary>
    /// Disposes of browser resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Context != null)
            await Context.DisposeAsync();

        if (Browser != null)
            await Browser.DisposeAsync();

        Playwright?.Dispose();
        GC.SuppressFinalize(this);
    }
}
