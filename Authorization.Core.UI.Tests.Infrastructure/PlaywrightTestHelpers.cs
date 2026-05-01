using Microsoft.Playwright;
using System.Web;

#pragma warning disable CA1054 // URI-like parameters should not be strings

namespace Authorization.Core.UI.Tests.Infrastructure;

public class PlaywrightTestHelpers(string baseUrl)
{
    /// <summary>
    /// Performs an automated login operation on the specified page using the provided user credentials.
    /// </summary>
    /// <param name="page">The page instance on which the login process is performed. Cannot be null.</param>
    /// <param name="login">The user credentials to use for logging in. Cannot be null.</param>
    /// <param name="returnUrl">An optional URL to redirect to after a successful login. If null, the default post-login page is used.</param>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    public async Task LogUserInAsync(IPage page, Login login, string? returnUrl = null)
    {
        ArgumentNullException.ThrowIfNull(page, nameof(page));
        ArgumentNullException.ThrowIfNull(login, nameof(login));

        var url = $"{baseUrl}/Identity/Account/Login";
        if (returnUrl != null)
        {
            url += "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl);
        }

        await page.GotoAsync(url).ConfigureAwait(false);
        await page.GetByPlaceholder("name@example.com").FillAsync(login.Email).ConfigureAwait(false);
        await page.GetByPlaceholder("password").FillAsync(login.Password).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Registers a new user with the specified email address and logs them in using a predefined password.
    /// </summary>
    /// <remarks>This method automates the registration and login process for testing purposes. The password
    /// used is predefined and not configurable. The method navigates through registration, confirmation, and login
    /// pages, and waits for network idle states to ensure completion of each step.</remarks>
    /// <param name="page">The page interface used to automate registration and login actions. Must not be null.</param>
    /// <param name="userEmail">The email address to use for user registration and login. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous registration and login operation.</returns>
    public async Task RegisterAndLogUserInAsync(IPage page, string userEmail)
    {
        ArgumentNullException.ThrowIfNull(page, nameof(page));
        ArgumentException.ThrowIfNullOrEmpty(userEmail, nameof(userEmail));

        var password = "Test123!@#";

        // Register
        await page.GotoAsync($"{baseUrl}/Identity/Account/Register").ConfigureAwait(false);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle).ConfigureAwait(false);

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync(userEmail).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password", Exact = true }).FillAsync(password).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Confirm Password" }).FillAsync(password).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync().ConfigureAwait(false);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle).ConfigureAwait(false);

        await page.GetByRole(AriaRole.Heading, new() { Name = "Register confirmation" }).IsVisibleAsync().ConfigureAwait(false);
        await page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm" }).ClickAsync().ConfigureAwait(false);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle).ConfigureAwait(false);

        await page.GotoAsync($"{baseUrl}/Identity/Account/Login").ConfigureAwait(false);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle).ConfigureAwait(false);

        await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync(userEmail).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(password).ConfigureAwait(false);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync().ConfigureAwait(false);

        await page.WaitForURLAsync(baseUrl, new() { Timeout = 10_000 }).ConfigureAwait(false);
    }
}
