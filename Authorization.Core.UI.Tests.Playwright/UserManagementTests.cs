using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Playwright.Infrastructure;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace Authorization.Core.UI.Tests.Playwright;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
internal class UserManagementTests : PageTest
{
    private HttpClient _httpClient;
    private WebAppFactory _webAppFactory;

    #region Setup / Teardown

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _webAppFactory = new WebAppFactory();
        _webAppFactory.EnsureUser(Logins.UserManager, nameof(SysUiGuids.Role.UserManager));
    }

    [SetUp]
    public async Task SetupAsync()
    {
        _httpClient = _webAppFactory.CreateClient();

        await Context.ConfigureRouteAsync(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _webAppFactory?.Dispose();
    }

    #endregion

    [Test]
    [Description("Can create new user with role")]
    public async Task Can_create_new_user_with_role()
    {
        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Create User"));

        var userEmail = "Test01User@company.com";
        var userPassword = "SuperSecret01!";
        var userPhoneNumber = "(123) 456-7890";
        var userGivenName = "Test";
        var userSurname = "User";
        var userRoleName = nameof(SysUiGuids.Role.RoleManager);

        await Page.GetByLabel("Email").FillAsync(userEmail);
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
        Assert.That(title, Does.Contain("User Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{userEmail}' successfully created." })
            ).ToHaveCountAsync(1);

        var user = await _webAppFactory.GetUserByEmailAsync(userEmail);
        Assert.That(user, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(user.PhoneNumber, Is.EqualTo(userPhoneNumber));
            Assert.That(user.GivenName, Is.EqualTo(userGivenName));
            Assert.That(user.Surname, Is.EqualTo(userSurname));
            Assert.That(user.Claims.Any(c => c.ClaimValue == userRoleName));
        });
    }

    [Test]
    [Description("Can display existing user")]
    public async Task Can_display_existing_user()
    {
        var user = new ApplicationUser
        {
            Email = "Test02User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890"
        };
        var userRoleName = nameof(SysUiGuids.Role.UserManager);

        await _webAppFactory.EnsureUserAsync(user, "SuperSecret02!", userRoleName);

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "View" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Details"));

        await Expect(Page.GetByLabel("Id")).ToHaveValueAsync(user.Id);
        await Expect(Page.GetByLabel("Email", new() { Exact = true })).ToHaveValueAsync(user.Email);
        await Expect(Page.GetByLabel("First Name")).ToHaveValueAsync(user.GivenName);
        await Expect(Page.GetByLabel("Last Name")).ToHaveValueAsync(user.Surname);
        await Expect(Page.GetByLabel("Phone Number", new() { Exact = true })).ToHaveValueAsync(user.PhoneNumber);
        await Expect(Page.Locator("#UserModel_LockoutEnd")).ToHaveValueAsync(user.LockoutEnd?.ToString() ?? string.Empty);
        await Expect(Page.GetByLabel("Failed Logins")).ToHaveValueAsync(user.AccessFailedCount.ToString() ?? string.Empty);

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        await Expect(rows).ToHaveCountAsync(1);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRoleName })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.That(isChecked, Is.True);
    }

    [Test]
    [Description("Can update existing user and role")]
    public async Task Can_update_existing_user_and_role()
    {
        var user = new ApplicationUser
        {
            Email = "Test03User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890",
            LockoutEnd = DateTimeOffset.UtcNow.AddDays(1)
        }.SetClaims(nameof(SysUiGuids.Role.RoleManager));

        await _webAppFactory.EnsureUserAsync(user, "SuperSecret03!");

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Edit" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Edit User"));

        var locator = Page.GetByLabel("Email", new() { Exact = true });
        await Expect(locator).ToHaveValueAsync(user.Email!);
        user.Email = "Test03User1@company.com";
        await locator.FillAsync(user.Email);

        locator = Page.GetByLabel("First Name");
        await Expect(locator).ToHaveValueAsync(user.GivenName);
        user.GivenName = "Test";
        await locator.FillAsync(user.GivenName);

        locator = Page.GetByLabel("Last Name");
        await Expect(locator).ToHaveValueAsync(user.Surname);
        user.Surname = "User";
        await locator.FillAsync(user.Surname);

        locator = Page.GetByLabel("Phone Number", new() { Exact = true });
        await Expect(locator).ToHaveValueAsync(user.PhoneNumber!);
        user.PhoneNumber = "(123) 456-7890";
        await locator.FillAsync(user.PhoneNumber);

        locator = Page.GetByLabel("Lockout Ends On (UTC)");
        await Expect(locator).ToHaveValueAsync($"{user.LockoutEnd!.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff").TrimEnd('0')}");
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
        Assert.That(title, Does.Contain("User Management"));
        locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{user.Email}' successfully updated." });
        Assert.That(locator, Is.Not.Null);

        var dbUser = await _webAppFactory.GetUserByIdAsync(user.Id);
        Assert.That(dbUser, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dbUser.Email, Is.EqualTo(user.Email));
            Assert.That(dbUser.GivenName, Is.EqualTo(user.GivenName));
            Assert.That(dbUser.PhoneNumber, Is.EqualTo(user.PhoneNumber));
            Assert.That(dbUser.Surname, Is.EqualTo(user.Surname));
            Assert.That(dbUser.LockoutEnd, Is.EqualTo(user.LockoutEnd));
        });

        Assert.That(dbUser.Claims, Has.Count.EqualTo(1));
        Assert.That(dbUser.Claims.Any(c => c.ClaimValue == nameof(SysUiGuids.Role.UserManager)), Is.True);
    }

    [Test]
    [Description("Can delete existing user")]
    public async Task Can_delete_existing_user()
    {
        var userRoleName = nameof(SysUiGuids.Role.RoleManager);
        var user = new ApplicationUser
        {
            Email = "Test04User@company.com",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "(123) 456-7890",
            LockoutEnd = DateTime.UtcNow.AddDays(1)
        }.SetClaims(userRoleName);

        await _webAppFactory.EnsureUserAsync(user, "SuperSecret04!");

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete User"));

        await Expect(Page.GetByLabel("Id")).ToHaveValueAsync(user.Id);
        await Expect(Page.Locator("#UserModel_Email").Nth(1)).ToHaveValueAsync(user.Email ?? string.Empty);
        await Expect(Page.GetByLabel("First Name")).ToHaveValueAsync(user.GivenName);
        await Expect(Page.GetByLabel("Last Name")).ToHaveValueAsync(user.Surname);
        await Expect(Page.GetByLabel("Phone Number", new() { Exact = true })).ToHaveValueAsync(user.PhoneNumber ?? string.Empty);
        await Expect(Page.Locator("#UserModel_LockoutEnd")).ToHaveValueAsync(user.LockoutEnd?.ToString() ?? string.Empty);
        await Expect(Page.GetByLabel("Failed Logins")).ToHaveValueAsync(user.AccessFailedCount.ToString() ?? string.Empty);

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        await Expect(rows).ToHaveCountAsync(1);

        var isChecked = await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = userRoleName })
            .GetByRole(AriaRole.Checkbox)
            .IsCheckedAsync();
        Assert.That(isChecked, Is.True);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{user.Email}' successfully deleted." })
            ).ToHaveCountAsync(1);

        Assert.That(await _webAppFactory.GetUserByIdAsync(user.Id), Is.Null);
    }

    [Test]
    [Description("Delete button is disabled for system user")]
    public async Task Delete_button_is_disabled_for_system_user()
    {
        var user = await _webAppFactory.GetUserByIdAsync(SysGuids.User.Administrator);

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete User"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = "System accounts may not be deleted!" })
            ).ToHaveCountAsync(1);

        await Expect(
            Page.GetByRole(AriaRole.Button, new() { Name = "Delete" })
            ).ToBeDisabledAsync();
    }

    [Test]
    [Description("Index page displays High Severity notification")]
    public async Task Index_page_displays_high_severity_notification()
    {
        var user = new ApplicationUser { Email = "Test06User@company.com" };
        await _webAppFactory.EnsureUserAsync(user, "SuperSecret06!");

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = user!.Email })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete User"));

        await _webAppFactory.DeleteUserAsync(user.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Error: User '{user.Email}' was not found in the database." })
            ).ToHaveCountAsync(1);
    }

    [Test]
    [Description("New user can log in")]
    public async Task New_user_can_log_in()
    {
        var login = new Login("Test07User@company.com", "SuperSecret07!");

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Create User"));

        await Page.GetByLabel("Email").FillAsync(login.Email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(login.Password);
        await Page.GetByLabel("Confirm password").FillAsync(login.Password);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"User '{login.Email}' successfully created." })
            ).ToHaveCountAsync(1);
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GotoAsync(Extensions.HostUri);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Log out"));

        await Page.LogUserInAsync(login, "/Admin");
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Administration"));

        await Expect(Page.GetByTitle("Manage")).ToContainTextAsync(login.Email);
    }

    [Test]
    [Description("Create User returns validation errors for bad password")]
    public async Task Create_User_returns_validation_errors_for_bad_password()
    {
        var login = new Login("Test08User@company.com", "bad_password");

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/User");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("User Management"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Create User"));

        await Page.GetByLabel("Email").FillAsync(login.Email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(login.Password);
        await Page.GetByLabel("Confirm password").FillAsync(login.Password);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Create User"));

        var errors = await Page.Locator(".validation-summary-errors").AllInnerTextsAsync();
        Assert.Multiple(() =>
        {
            Assert.That(errors.Any(m => m.Contains("one digit")), Is.True);
            Assert.That(errors.Any(m => m.Contains("one upper")), Is.True);
        });
    }
}
