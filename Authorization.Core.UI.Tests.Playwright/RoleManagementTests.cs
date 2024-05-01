using Authorization.Core.UI.Test.Web;
using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Playwright.Infrastructure;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Authorization.Core.UI.Tests.Playwright;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class RoleManagementTests : PageTest
{
    private HttpClient _httpClient;
    private WebAppFactory _webAppFactory;

    #region Setup / Teardown

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _webAppFactory = new WebAppFactory();
        _webAppFactory.EnsureUser(Logins.RoleManager, nameof(SysUiGuids.Role.RoleManager));
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
    [Description("Can create new role with claims")]
    public async Task Can_create_new_role_with_claims()
    {
        await Page.LogUserInAsync(Logins.Administrator, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Create Role"));

        var role = new ApplicationRole
        {
            Name = "Test01Role", Description = "Test Role"
        }.SetClaims("Calendar.List", "Document.List", "Role.List", "User.List");

        await Page.GetByLabel("Name").FillAsync(role.Name!);
        await Page.GetByLabel("Description").FillAsync(role.Description);
        await Page.GetByLabel("Search:").FillAsync("List");
        foreach (var claim in role.Claims)
        {
            await Page.GetByRole(AriaRole.Row, new() { Name = claim.ClaimValue })
                .GetByRole(AriaRole.Checkbox).CheckAsync();
        }
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' successfully created." })
            ).ToHaveCountAsync(1);

        var dbRole = await _webAppFactory.GetRoleByNameAsync(role.Name!);
        Assert.That(dbRole, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dbRole.Name, Is.EqualTo(role.Name));
            Assert.That(dbRole.Description, Is.EqualTo(role.Description));
            Assert.That(role.Claims.Select(c => c.ClaimValue!).Order(),
                Is.EqualTo(dbRole.Claims.Select(c => c.ClaimValue!).Order())
                .AsCollection);
        });
    }

    [Test]
    [Description("Can display existing role")]
    public async Task Can_display_existing_role()
    {
        var role = await _webAppFactory.GetRoleByIdAsync(SysUiGuids.Role.UserManager);

        await Page.LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "View" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Details"));

        await Expect(Page.GetByLabel("Id")).ToHaveValueAsync(role.Id);
        await Expect(Page.GetByLabel("Name")).ToHaveValueAsync(role.Name!);
        await Expect(Page.GetByLabel("Description")).ToHaveValueAsync(role.Description!);

        var roleClaims = role.Claims
            .Select(r => r.ClaimValue!)
            .ToList();

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        var count = await rows.CountAsync();
        Assert.That(count, Is.EqualTo(roleClaims.Count));
        foreach (var claim in roleClaims)
        {
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = claim, Exact = true })).ToHaveCountAsync(1);
        }
    }

    [Test]
    [Description("Can update existing role and claims")]
    public async Task Can_update_existing_role_and_claims()
    {
        var role = new ApplicationRole
        {
            Name = "Test03Role",
            Description = "Can list and view news items."
        }.SetClaims(AppClaims.Calendar.List, AppClaims.Calendar.Read);
        await _webAppFactory.EnsureRoleAsync(role);

        await Page.LogUserInAsync(Logins.Administrator, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Edit" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Edit Role"));

        var locator = Page.GetByLabel(nameof(role.Name));
        await Expect(locator).ToHaveValueAsync(role.Name ?? string.Empty);
        role.Name = "Test03RoleX";
        await locator.FillAsync(role.Name);

        locator = Page.GetByLabel(nameof(role.Description));
        await Expect(locator).ToHaveValueAsync(role.Description ?? string.Empty);
        role.Description = "Can create, delete, list, read, and update news items.";
        await locator.FillAsync(role.Description);

        await Page.GetByLabel("Search:").FillAsync("Calendar");

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.List }).GetByRole(AriaRole.Checkbox);
        await Expect(locator).ToBeCheckedAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Read }).GetByRole(AriaRole.Checkbox);
        await Expect(locator).ToBeCheckedAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Create }).GetByRole(AriaRole.Checkbox);
        await Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Delete }).GetByRole(AriaRole.Checkbox);
        await Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Update }).GetByRole(AriaRole.Checkbox);
        await Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));
        locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' was successfully updated." });
        Assert.That(locator, Is.Not.Null);

        role.SetClaims(AppClaims.Calendar.DefinedClaims);

        var dbRole = await _webAppFactory.GetRoleByIdAsync(role.Id);
        Assert.That(dbRole, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dbRole.Name, Is.EqualTo(role.Name));
            Assert.That(dbRole.Description, Is.EqualTo(role.Description));
            Assert.That(role.Claims.Select(c => c.ClaimValue!).Order(),
                Is.EqualTo(dbRole.Claims.Select(c => c.ClaimValue!).Order())
                .AsCollection);
        });
    }

    [Test]
    [Description("Can delete existing role")]
    public async Task Can_delete_existing_role()
    {
        var role = new ApplicationRole
        {
            Name = "Test04Role",
            Description = "Can create, delete, list, read, and update calendar items."
        }.SetClaims(AppClaims.Calendar.DefinedClaims);
        await _webAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test04User@company.com", "Test04pa$$");
        await _webAppFactory.EnsureUserAsync(login, role.Name);

        await Page.LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete Role"));

        var locator = Page.GetByRole(AriaRole.Textbox).Nth(0);
        await Expect(locator).ToHaveValueAsync(role.Id);

        locator = Page.GetByRole(AriaRole.Textbox).Nth(1);
        await Expect(locator).ToHaveValueAsync(role.Name!);

        locator = Page.GetByRole(AriaRole.Textbox).Nth(2);
        await Expect(locator).ToHaveValueAsync(role.Description);

        // Verify assigned User is displayed
        locator = Page.GetByRole(AriaRole.Cell, new() { Name = login.Email });
        await Expect(locator).ToHaveCountAsync(1);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' successfully deleted." })
            ).ToHaveCountAsync(1);
    }

    [Test]
    [Description("Delete button is disabled for system role")]
    public async Task Delete_button_is_disabled_for_system_role()
    {
        await Page.LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = nameof(AppGuids.Role.DocumentManager) })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete Role"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = "System Roles may not be deleted!" })
            ).ToHaveCountAsync(1);

        await Expect(
            Page.GetByRole(AriaRole.Button, new() { Name = "Delete" })
            ).ToBeDisabledAsync();
    }

    [Test]
    [Description("Index page displays High Severity notification")]
    public async Task Index_page_displays_High_Severity_notification()
    {
        var role = new ApplicationRole { Name = "Test06Role" };
        await _webAppFactory.EnsureRoleAsync(role);

        await Page.LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Delete Role"));

        await _webAppFactory.DeleteRoleAsync(role.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.That(title, Does.Contain("Role Management"));

        await Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Error: Role '{role.Name}' was not found in the database." })
            ).ToHaveCountAsync(1);
    }
}