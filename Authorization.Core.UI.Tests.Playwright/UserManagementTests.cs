using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Infrastructure;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace Authorization.Core.UI.Tests.Playwright;

[Collection("Playwright")]
public class UserManagementTests(PlaywrightFixture playwrightFixture) : IAsyncLifetime
{

    public string BaseUrl { get; private set; } = null!;

    private IPage Page { get; set; } = null!;

    public PlaywrightTestHelpers TestHelpers { get; private set; } = null!;

    public Dictionary<string, ApplicationRole> RoleDictionary { get; private set; } = null!;

    public WebAppFactory WebAppFactory { get; private set; } = null!;


    public async ValueTask InitializeAsync()
    {
        Page = await playwrightFixture.CreatePageAsync();
        WebAppFactory = new WebAppFactory();
        WebAppFactory.UseKestrel();
        WebAppFactory.StartServer();

        BaseUrl = WebAppFactory.ClientOptions.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        TestHelpers = new PlaywrightTestHelpers(BaseUrl);
        WebAppFactory.EnsureUser(Logins.UserManager, SysUiGuids.Role.UserManager);

        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        RoleDictionary = await dbContext.Roles.ToDictionaryAsync(ar => ar.Id, ar => ar);
    }

    public async ValueTask DisposeAsync()
    {
        await Page.CloseAsync();
        await WebAppFactory.DisposeAsync();
        GC.SuppressFinalize(this);
    }


    [Fact(DisplayName = "Can create new user with role")]
    public async Task Test001()
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
        var userRole = RoleDictionary[SysUiGuids.Role.RoleManager];

        await Page.GetByLabel("Email").FillAsync(userEmail);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(userPassword);
        await Page.GetByLabel("Confirm password").FillAsync(userPassword);
        await Page.GetByLabel("First Name").FillAsync(userGivenName);
        await Page.GetByLabel("Last Name").FillAsync(userSurname);
        await Page.GetByLabel("Phone Number").FillAsync(userPhoneNumber);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRole.Name })
            .GetByRole(AriaRole.Checkbox)
            .CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{userEmail}' successfully created." })
            ).ToHaveCountAsync(1);

        var user = await WebAppFactory.GetUserByEmailAsync(userEmail);
        Assert.NotNull(user);
        Assert.Multiple(() =>
        {
            Assert.Equal(userPhoneNumber, user.PhoneNumber);
            Assert.Equal(userGivenName, user.GivenName);
            Assert.Equal(userSurname, user.Surname);
            Assert.Contains(user.Claims, c => c.ClaimValue == userRole.Id);
        });
    }

    [Fact(DisplayName = "Can display existing user")]
    public async Task Test002()
    {
        var user = new ApplicationUser
        {
            Email = "Test02User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890"
        };
        var userRole = RoleDictionary[SysUiGuids.Role.UserManager];

        await WebAppFactory.EnsureUserAsync(user, "SuperSecret02!", userRole.Id);

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByLabel("View")
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Details", title);

        await Assertions.Expect(Page.GetByLabel("Id")).ToHaveValueAsync(user.Id);
        await Assertions.Expect(Page.GetByLabel("Email", new() { Exact = true })).ToHaveValueAsync(user.Email);
        await Assertions.Expect(Page.GetByLabel("First Name")).ToHaveValueAsync(user.GivenName);
        await Assertions.Expect(Page.GetByLabel("Last Name")).ToHaveValueAsync(user.Surname);
        await Assertions.Expect(Page.GetByLabel("Phone Number", new() { Exact = true })).ToHaveValueAsync(user.PhoneNumber);
        await Assertions.Expect(Page.Locator("#UserModel_LockoutEnd")).ToHaveValueAsync(user.LockoutEnd?.ToString() ?? string.Empty);
        await Assertions.Expect(Page.GetByLabel("Failed Logins")).ToHaveValueAsync(user.AccessFailedCount.ToString() ?? string.Empty);

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        await Assertions.Expect(rows).ToHaveCountAsync(1);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRole.Name })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.True(isChecked);
    }

    [Fact(DisplayName = "Can update existing user and role")]
    public async Task Test003()
    {
        var user = new ApplicationUser
        {
            Email = "Test03User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890",
            LockoutEnd = DateTimeOffset.UtcNow.AddDays(1)
        }.SetClaims(SysUiGuids.Role.RoleManager);

        await WebAppFactory.EnsureUserAsync(user, "SuperSecret03!");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByLabel("Edit")
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Edit User", title);

        var locator = Page.GetByLabel("Email", new() { Exact = true });
        await Assertions.Expect(locator).ToHaveValueAsync(user.Email!);
        user.Email = "Test03User1@company.com";
        await locator.FillAsync(user.Email);

        locator = Page.GetByLabel("First Name");
        await Assertions.Expect(locator).ToHaveValueAsync(user.GivenName);
        user.GivenName = "Test";
        await locator.FillAsync(user.GivenName);

        locator = Page.GetByLabel("Last Name");
        await Assertions.Expect(locator).ToHaveValueAsync(user.Surname);
        user.Surname = "User";
        await locator.FillAsync(user.Surname);

        locator = Page.GetByLabel("Phone Number", new() { Exact = true });
        await Assertions.Expect(locator).ToHaveValueAsync(user.PhoneNumber!);
        user.PhoneNumber = "(123) 456-7890";
        await locator.FillAsync(user.PhoneNumber);

        locator = Page.GetByLabel("Lockout Ends On (UTC)");
        await Assertions.Expect(locator).ToHaveValueAsync($"{user.LockoutEnd!.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff").TrimEnd('0')}");
        user.LockoutEnd = null;
        await locator.FillAsync(string.Empty);

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

        var dbUser = await WebAppFactory.GetUserByIdAsync(user.Id);
        Assert.NotNull(dbUser);
        Assert.Multiple(() =>
        {
            Assert.Equal(user.Email, dbUser.Email);
            Assert.Equal(user.GivenName, dbUser.GivenName);
            Assert.Equal(user.PhoneNumber, dbUser.PhoneNumber);
            Assert.Equal(user.Surname, dbUser.Surname);
            Assert.Equal(user.LockoutEnd, dbUser.LockoutEnd);
        });

        Assert.Single(dbUser.Claims);
        Assert.Contains(dbUser.Claims, c => c.ClaimValue == SysUiGuids.Role.UserManager);
    }

    [Fact(DisplayName = "Can delete existing user")]
    public async Task Test004()
    {
        var userRole = RoleDictionary[SysUiGuids.Role.RoleManager];
        var user = new ApplicationUser
        {
            Email = "Test04User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890",
            LockoutEnd = DateTime.UtcNow.AddDays(1)
        }.SetClaims(userRole.Id);
        await WebAppFactory.EnsureUserAsync(user, "SuperSecret04!");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user.Email })
            .GetByLabel("Delete")
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        await Assertions.Expect(Page.GetByLabel("Id")).ToHaveValueAsync(user.Id);
        await Assertions.Expect(Page.Locator("#UserModel_Email").Nth(1)).ToHaveValueAsync(user.Email ?? string.Empty);
        await Assertions.Expect(Page.GetByLabel("First Name")).ToHaveValueAsync(user.GivenName);
        await Assertions.Expect(Page.GetByLabel("Last Name")).ToHaveValueAsync(user.Surname);
        await Assertions.Expect(Page.GetByLabel("Phone Number", new() { Exact = true })).ToHaveValueAsync(user.PhoneNumber ?? string.Empty);
        await Assertions.Expect(Page.Locator("#UserModel_LockoutEnd")).ToHaveValueAsync(user.LockoutEnd?.ToString() ?? string.Empty);
        await Assertions.Expect(Page.GetByLabel("Failed Logins")).ToHaveValueAsync(user.AccessFailedCount.ToString() ?? string.Empty);

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        await Assertions.Expect(rows).ToHaveCountAsync(1);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRole.Name })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.True(isChecked);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{user.Email}' successfully deleted." })
            ).ToHaveCountAsync(1);

        Assert.Null(await WebAppFactory.GetUserByIdAsync(user.Id));
    }

    [Fact(DisplayName = "Delete button is disabled for system user")]
    public async Task Test005()
    {
        var user = await WebAppFactory.GetUserByIdAsync(SysGuids.User.Administrator);

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByLabel("Delete")
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = "System accounts may not be deleted!" })
            ).ToHaveCountAsync(1);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Button, new() { Name = "Delete" })
            ).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "Index page displays High Severity notification")]
    public async Task Test006()
    {
        var user = new ApplicationUser { Email = "Test06User@company.com" };
        await WebAppFactory.EnsureUserAsync(user, "SuperSecret06!");

        await LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByLabel("Delete")
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete User", title);

        await WebAppFactory.DeleteUserAsync(user.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Error: User '{user.Email}' was not found in the database." })
            ).ToHaveCountAsync(1);
    }

    [Fact(DisplayName = "New user can log in")]
    public async Task Test007()
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

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{login.Email}' successfully created." })
            ).ToHaveCountAsync(1);
        title = await Page.TitleAsync();
        Assert.Contains("User Management", title);
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Log out", title);

        await LogUserInAsync(login, "/Admin");
        title = await Page.TitleAsync();
        Assert.Contains("Administration", title);

        await Assertions.Expect(Page.GetByTitle("Manage")).ToContainTextAsync(login.Email);
    }

    [Fact(DisplayName = "Create User returns validation errors for bad password")]
    public async Task Test008()
    {
        var login = new Login("Test08User@company.com", "bad_password");

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
        Assert.Contains("Create User", title);

        var errors = await Page.Locator(".validation-summary-errors").AllInnerTextsAsync();
        Assert.Multiple(() =>
        {
            Assert.Contains(errors, m => m.Contains("one digit"));
            Assert.Contains(errors, m => m.Contains("one upper"));
        });
    }

    [Fact(DisplayName = "Index page Create link disabled when Create claim missing")]
    public async Task Test009()
    {
        var role = new ApplicationRole
        {
            Name = "Test09Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.User.DefinedClaims.Except([SysClaims.User.Create]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test09User@company.com", "Test09pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Link, new() { Name = "Create New" });
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "Index page Edit button disabled when Update claim missing")]
    public async Task Test010()
    {
        var role = new ApplicationRole
        {
            Name = "Test10Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.User.DefinedClaims.Except([SysClaims.User.Update]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test10User@company.com", "Test10pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = login.Email })
            .GetByLabel("Edit");
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "View button disabled on Index page when Read claim missing")]
    public async Task Test011()
    {
        var role = new ApplicationRole
        {
            Name = "Test11Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.User.DefinedClaims.Except([SysClaims.User.Read]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test11User@company.com", "Test11pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = login.Email })
            .GetByLabel("View");
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "View button disabled on Index page when Delete claim missing")]
    public async Task Test012()
    {
        var role = new ApplicationRole
        {
            Name = "Test12Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.User.DefinedClaims.Except([SysClaims.User.Delete]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test12User@company.com", "Test12pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.Contains("User Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = login.Email })
            .GetByLabel("Delete");
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }


    /// <summary>
    /// Helper method to log a user in using the provided login credentials and optional return URL.
    /// </summary>
    /// <param name="login">The login credentials to use for authentication.</param>
    /// <param name="returnUrl">An optional URL to redirect to after a successful login.</param>
    /// <returns>A task that represents the asynchronous login operation.</returns>
    private async Task LogUserInAsync(Login login, string? returnUrl = null)
    {
        await TestHelpers.LogUserInAsync(Page, login, returnUrl);
    }
}
