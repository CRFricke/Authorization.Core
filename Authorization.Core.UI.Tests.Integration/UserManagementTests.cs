using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using Authorization.Core.UI.Tests.Integration.Infrastructure.Playwright;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Web;
using Xunit.Abstractions;

using static Authorization.Core.UI.Tests.Integration.Infrastructure.PlaywrightTestFixture;

namespace Authorization.Core.UI.Tests.Integration;

public class UserManagementTests : PageTest, IClassFixture<PlaywrightTestFixture>, IAsyncLifetime
{
    internal UserManagementTests(PlaywrightTestFixture fixture) : base(fixture.Browser)
    {
        Fixture = fixture;
        WebAppFactory = Fixture.WebAppFactory;
        Client = WebAppFactory.CreateClient(new() { BaseAddress = new(HostUri) });
    }

    public UserManagementTests(PlaywrightTestFixture fixture, ITestOutputHelper outputHelper) : this(fixture)
    {
        OutputHelper = outputHelper;
    }

    private ITestOutputHelper OutputHelper { get; } = null!;

    private PlaywrightTestFixture Fixture { get; }

    private HttpClient Client { get; }

    private WebAppFactory WebAppFactory { get; }

    private string HostUri { get; } = "https://localhost/";


    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await Context.RouteAsync($"{HostUri}**", async route =>
        {
            var request = route.Request;
            var content = request.PostDataBuffer is { } postDataBuffer
                ? new ByteArrayContent(postDataBuffer) : null;

            var requestMessage = new HttpRequestMessage(new(request.Method), request.Url)
            {
                Content = content
            };

            foreach (var header in request.Headers)
            {
                try
                {
                    if (header.Key.StartsWith("content-"))
                    {
                        requestMessage.Content!.Headers.Add(header.Key, header.Value);
                    }
                    else
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
                catch (Exception ex)
                {
                    OutputHelper.WriteLine("Error copying HTTP request headers in Playwright.BrowserContext Route handler.");
                    OutputHelper.WriteLine(ex.ToString());
                }
            }

            var response = await Client.SendAsync(requestMessage);
            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var responseHeaders = response.Content.Headers
                .Select(h => KeyValuePair.Create(h.Key, h.Value.First()));

            await route.FulfillAsync(new()
            {
                BodyBytes = responseBody,
                Headers = responseHeaders,
                Status = (int)response.StatusCode
            });
        });
    }

    public override async Task DisposeAsync()
    {
        Client?.Dispose();

        await base.DisposeAsync();
    }


    #region Helper Methods

    private async Task DeleteUserAsync(string userId)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dbUser = await dbContext.Users.FindAsync(userId);
        if (dbUser is not null)
        {
            dbContext.Users.Remove(dbUser);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    private async Task<ApplicationUser?> GetUserByEmailAsync(string userEmail)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users
            .Where(u => u.Email == userEmail)
            .Include(u => u.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    private async Task LogUserInAsync(Login login, string? returnUrl = null)
    {
        var url = $"{HostUri}Identity/Account/Login";
        if (returnUrl != null)
        {
            url += "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl);
        }

        await Page.GotoAsync(url);
        await Page.GetByPlaceholder("name@example.com").FillAsync(login.Email);
        await Page.GetByPlaceholder("password").FillAsync(login.Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }

    #endregion


    [Fact(DisplayName = "Can create new user with role")]
    public async Task UserManagementTest01Async()
    {
        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Create User", title);

        var userEmail = "Test01User@company.com";
        var userPassword = "SuperSecret01!";
        var userPhoneNumber = "(123) 456-7890";
        var userGivenName = "Test";
        var userSurname = "User";
        var userRoleName = nameof(SysUiGuids.Role.RoleManager);

        await Page.GetByLabel("Email").FillAsync("Test01User@company.com");
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(userPassword);
        await Page.GetByLabel("Confirm password").FillAsync(userPassword);
        await Page.GetByLabel("First Name").FillAsync(userGivenName);
        await Page.GetByLabel("Last Name").FillAsync(userSurname);
        await Page.GetByLabel("Phone Number").FillAsync(userPhoneNumber);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRoleName })
            .GetByRole(AriaRole.Checkbox)
            .CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{userEmail}' successfully created." });
        Assert.Equal(1, await locator.CountAsync());

        await Task.Delay(100);

        var user = await GetUserByEmailAsync(userEmail);
        Assert.NotNull(user);
        Assert.Equal(user.PhoneNumber, userPhoneNumber);
        Assert.Equal(user.GivenName, userGivenName);
        Assert.Equal(user.Surname, userSurname);
        Assert.Contains(user.Claims, c => c.ClaimValue == userRoleName);
    }

    [Fact(DisplayName = "Can display existing user")]
    public async void UserManagementTest02Async()
    {
        var user = new ApplicationUser
        { 
            Email = "Test02User@company.com", GivenName = "Test", Surname = "User", PhoneNumber = "(123) 456-7890"
        };
        var userRoleName = nameof(SysUiGuids.Role.UserManager);

        await Fixture.EnsureUserAsync(user, "SuperSecret02!", userRoleName);

        await LogUserInAsync(Logins.UserManager, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "View" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Details", title);

        var value = await Page.GetByLabel("Id").InputValueAsync();
        Assert.Equal(user.Id, value);
        value = await Page.GetByLabel("Email", new() { Exact = true }).InputValueAsync();
        Assert.Equal(user.Email, value);
        value = await Page.GetByLabel("First Name").InputValueAsync();
        Assert.Equal(user.GivenName, value);
        value = await Page.GetByLabel("Last Name").InputValueAsync();
        Assert.Equal(user.Surname, value);
        value = await Page.GetByLabel("Phone Number", new() { Exact = true }).InputValueAsync();
        Assert.Equal(user.PhoneNumber, value);
        value = await Page.Locator("#UserModel_LockoutEnd").First.InputValueAsync();
        Assert.Equal(user.LockoutEnd?.ToString() ?? string.Empty, value);
        value = await Page.GetByLabel("Failed Logins").InputValueAsync();
        Assert.Equal(user.AccessFailedCount, int.Parse(value));

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        var count = await rows.CountAsync();
        Assert.Equal(1, count);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRoleName })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.True(isChecked);
    }

    [Fact(DisplayName = "Can update existing user and role")]
    public async void UserManagementTest03Async()
    {
        var user = new ApplicationUser
        { 
            Email = "Test03User@company.com"
        }.SetClaims(nameof(SysUiGuids.Role.RoleManager));
        await Fixture.EnsureUserAsync(user, "SuperSecret03!");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Edit" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Edit User", title);

        var locator = Page.GetByLabel("Email", new() { Exact = true });
        var value = await locator.InputValueAsync();
        Assert.Equal(user.Email, value);
        user.Email = "Test03User1@company.com";
        await locator.FillAsync(user.Email);

        locator = Page.GetByLabel("First Name");
        value = await locator.InputValueAsync();
        Assert.Equal(user.GivenName ?? string.Empty, value);
        user.GivenName = "Test";
        await locator.FillAsync(user.GivenName);

        locator = Page.GetByLabel("Last Name");
        value = await locator.InputValueAsync();
        Assert.Equal(user.Surname ?? string.Empty, value);
        user.Surname = "User";
        await locator.FillAsync(user.Surname);

        locator = Page.GetByLabel("Phone Number", new() { Exact = true });
        value = await locator.InputValueAsync();
        Assert.Equal(user.PhoneNumber ?? string.Empty, value);
        user.PhoneNumber = "(123) 456-7890";
        await locator.FillAsync(user.PhoneNumber);

        var rows = Page.GetByRole(AriaRole.Row);
        await rows.Filter(new() { HasText = nameof(SysUiGuids.Role.RoleManager) })
            .GetByRole(AriaRole.Checkbox)
            .UncheckAsync();
        await rows.Filter(new() { HasText = nameof(SysUiGuids.Role.UserManager) })
            .GetByRole(AriaRole.Checkbox)
            .CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{user.Email}' successfully updated." });
        Assert.NotNull(locator);

        await Task.Delay(100);

        var dbUser = await GetUserByIdAsync(user.Id);
        Assert.NotNull(dbUser);
        Assert.Equal(user.Email, dbUser.Email);
        Assert.Equal(user.GivenName, dbUser.GivenName);
        Assert.Equal(user.PhoneNumber, dbUser.PhoneNumber);
        Assert.Equal(user.Surname, dbUser.Surname);

        Assert.Single(dbUser.Claims);
        Assert.Contains(dbUser.Claims, c => c.ClaimValue == nameof(SysUiGuids.Role.UserManager));
    }

    [Fact(DisplayName = "Can delete existing user")]
    public async void UserManagementTest04Async()
    {
        var userRoleName = nameof(SysUiGuids.Role.RoleManager);
        var user = new ApplicationUser
        {
            Email = "Test04User@company.com", GivenName = "Test", Surname = "User", 
            PhoneNumber = "(123) 456-7890", LockoutEnd = DateTime.UtcNow.AddDays(1)
        }.SetClaims(userRoleName);
        await Fixture.EnsureUserAsync(user, "SuperSecret04!");

        await LogUserInAsync(Logins.UserManager, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        var value = await Page.GetByLabel("Id").InputValueAsync();
        Assert.Equal(user.Id, value);
        value = await Page.Locator("#UserModel_Email").Nth(1).InputValueAsync();
        Assert.Equal(user.Email, value);
        value = await Page.GetByLabel("First Name").InputValueAsync();
        Assert.Equal(user.GivenName, value);
        value = await Page.GetByLabel("Last Name").InputValueAsync();
        Assert.Equal(user.Surname, value);
        value = await Page.GetByLabel("Phone Number", new() { Exact = true }).InputValueAsync();
        Assert.Equal(user.PhoneNumber, value);
        value = await Page.Locator("#UserModel_LockoutEnd").First.InputValueAsync();
        Assert.Equal(user.LockoutEnd?.ToString() ?? string.Empty, value);
        value = await Page.GetByLabel("Failed Logins").InputValueAsync();
        Assert.Equal(user.AccessFailedCount, int.Parse(value));

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        var count = await rows.CountAsync();
        Assert.Equal(1, count);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRoleName })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.True(isChecked);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{user.Email}' successfully deleted." });
        Assert.Equal(1, await locator.CountAsync());

        Assert.Null(await GetUserByIdAsync(user.Id));
    }

    [Fact(DisplayName = "Delete button is disabled for system user")]
    public async void UserManagementTest05Async()
    {
        var user = await GetUserByIdAsync(SysGuids.User.Administrator);

        await LogUserInAsync(Logins.UserManager, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = "System accounts may not be deleted!" });
        Assert.Equal(1, await locator.CountAsync());

        locator = Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First;
        Assert.True(await locator.IsDisabledAsync());
    }

    [Fact(DisplayName = "Index page displays High Severity notification")]
    public async void UserManagementTest06Async()
    {
        var user = new ApplicationUser { Email = "Test06User@company.com" };
        await Fixture.EnsureUserAsync(user, "SuperSecret06!");

        await LogUserInAsync(Logins.UserManager, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        await DeleteUserAsync(user.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"Error: User '{user.Email}' was not found in the database." });
        Assert.Equal(1, await locator.CountAsync());
    }

    [Fact(DisplayName = "New user can log in")]
    public async Task UserManagementTest07Async()
    {
        var login = new Login("Test07User@company.com", "SuperSecret07!");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Create User", title);

        await Page.GetByLabel("Email").FillAsync(login.Email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(login.Password);
        await Page.GetByLabel("Confirm password").FillAsync(login.Password);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{login.Email}' successfully created." });
        await locator.WaitForAsync();
        Assert.Equal(1, await locator.CountAsync());

        await Page.GotoAsync(HostUri);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        await Task.Delay(100);

        await LogUserInAsync(login, "/Admin");
        title = await Page.TitleAsync();
        Assert.Contains("Administration", title);
    }

    [Fact(DisplayName = "Create User returns validation errors for bad password")]
    public async Task UserManagementTest08Async()
    {
        var login = new Login("Test08User@company.com", "bad_password");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Create User", title);

        await Page.Locator("#UserModel_Email").FillAsync(login.Email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(login.Password);
        await Page.GetByLabel("Confirm password").FillAsync(login.Password);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        title = await Page.TitleAsync();
        Assert.Contains("Create User", title);

        var locator = Page.Locator(".validation-summary-errors")
            .GetByRole(AriaRole.Listitem);
        Assert.Equal(2, await locator.CountAsync());
        var errMessages = await locator.AllInnerTextsAsync();
        Assert.Contains(errMessages, m => m.Contains("one digit"));
        Assert.Contains(errMessages, m => m.Contains("one upper"));
    }
}
