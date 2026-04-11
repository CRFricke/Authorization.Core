using Authorization.Core.UI.Test.Web;
using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Infrastructure;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using CRFricke.Test.Support.Infrastructure;
using Microsoft.Playwright;

namespace Authorization.Core.UI.Tests.Playwright;

[Collection("Playwright")]
public class RoleManagementTests(PlaywrightFixture playwrightFixture) : IAsyncLifetime
{
    public string BaseUrl { get; private set; } = null!;

    private IPage Page { get; set; } = null!;

    public PlaywrightTestHelpers TestHelpers { get; private set; } = null!;

    public WebAppFactory WebAppFactory { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Page = await playwrightFixture.CreatePageAsync();

        WebAppFactory = new WebAppFactory();
        WebAppFactory.UseKestrel();
        WebAppFactory.StartServer();

        BaseUrl = WebAppFactory.ClientOptions.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost";
        TestHelpers = new PlaywrightTestHelpers(BaseUrl);

        WebAppFactory.EnsureUser(Logins.RoleManager, SysUiGuids.Role.RoleManager);
    }

    public async ValueTask DisposeAsync()
    {
        await Page.CloseAsync();
        await WebAppFactory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Create Role [Post] sets assigned claims")]
    public async Task Test001()
    {
        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Create Role", title);

        var role = new ApplicationRole
        {
            Name = "Test01Role",
            Description = "Test Role"
        }.SetClaims(AppClaims.Calendar.List, AppClaims.Document.List, SysClaims.Role.List, SysClaims.User.List);

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
        Assert.Contains("Role Management", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' successfully created." })
            ).ToHaveCountAsync(1);

        var dbRole = await WebAppFactory.GetRoleByNameAsync(role.Name!);
        Assert.NotNull(dbRole);
        Assert.Multiple(() =>
        {
            Assert.Equal(role.Name, dbRole.Name);
            Assert.Equal(role.Description, dbRole.Description);
            Assert.Equal(role.Claims.Select(c => c.ClaimValue!).Order(), dbRole.Claims.Select(c => c.ClaimValue!).Order());
        });
    }

    [Fact(DisplayName = "Can display existing role")]
    public async Task Test002()
    {
        var role = await WebAppFactory.GetRoleByIdAsync(SysUiGuids.Role.UserManager);

        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "View" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Details", title);

        await Assertions.Expect(Page.GetByLabel("Id")).ToHaveValueAsync(role.Id);
        await Assertions.Expect(Page.GetByLabel("Name")).ToHaveValueAsync(role.Name!);
        await Assertions.Expect(Page.GetByLabel("Description")).ToHaveValueAsync(role.Description!);

        var roleClaims = role.Claims
            .Select(r => r.ClaimValue!)
            .ToList();

        var rows = Page.GetByRole(AriaRole.Row)
            .GetByRole(AriaRole.Checkbox, new() { Checked = true });
        var count = await rows.CountAsync();
        Assert.Equal(roleClaims.Count, count);
        foreach (var claim in roleClaims)
        {
            await Assertions.Expect(Page.GetByRole(AriaRole.Cell, new() { Name = claim, Exact = true })).ToHaveCountAsync(1);
        }
    }

    [Fact(DisplayName = "Can update existing role and claims")]
    public async Task Test003()
    {
        var role = new ApplicationRole
        {
            Name = "Test03Role",
            Description = "Can list and view news items."
        }.SetClaims(AppClaims.Calendar.List, AppClaims.Calendar.Read);
        await WebAppFactory.EnsureRoleAsync(role);

        await LogUserInAsync(Logins.Administrator, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Edit" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Edit Role", title);

        var locator = Page.GetByLabel(nameof(role.Name));
        await Assertions.Expect(locator).ToHaveValueAsync(role.Name ?? string.Empty);
        role.Name = "Test03RoleX";
        await locator.FillAsync(role.Name);

        locator = Page.GetByLabel(nameof(role.Description));
        await Assertions.Expect(locator).ToHaveValueAsync(role.Description ?? string.Empty);
        role.Description = "Can create, delete, list, read, and update news items.";
        await locator.FillAsync(role.Description);

        await Page.GetByLabel("Search:").FillAsync("Calendar");

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.List }).GetByRole(AriaRole.Checkbox);
        await Assertions.Expect(locator).ToBeCheckedAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Read }).GetByRole(AriaRole.Checkbox);
        await Assertions.Expect(locator).ToBeCheckedAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Create }).GetByRole(AriaRole.Checkbox);
        await Assertions.Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Delete }).GetByRole(AriaRole.Checkbox);
        await Assertions.Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.Calendar.Update }).GetByRole(AriaRole.Checkbox);
        await Assertions.Expect(locator).Not.ToBeCheckedAsync();
        await locator.CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);
        locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' was successfully updated." });
        Assert.NotNull(locator);

        role.SetClaims(AppClaims.Calendar.DefinedClaims);

        var dbRole = await WebAppFactory.GetRoleByIdAsync(role.Id);
        Assert.NotNull(dbRole);
        Assert.Multiple(() =>
        {
            Assert.Equal(role.Name, dbRole.Name);
            Assert.Equal(role.Description, dbRole.Description);
            Assert.Equal(role.Claims.Select(
                c => c.ClaimValue!).Order(),
                dbRole.Claims.Select(c => c.ClaimValue!).Order()
                );
        });
    }

    [Fact(DisplayName = "Can delete existing role")]
    public async Task Test004()
    {
        var role = new ApplicationRole
        {
            Name = "Test04Role",
            Description = "Can create, delete, list, read, and update calendar items."
        }.SetClaims(AppClaims.Calendar.DefinedClaims);
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test04User@company.com", "Test04pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete Role", title);

        var locator = Page.GetByRole(AriaRole.Textbox).Nth(0);
        await Assertions.Expect(locator).ToHaveValueAsync(role.Id);

        locator = Page.GetByRole(AriaRole.Textbox).Nth(1);
        await Assertions.Expect(locator).ToHaveValueAsync(role.Name!);

        locator = Page.GetByRole(AriaRole.Textbox).Nth(2);
        await Assertions.Expect(locator).ToHaveValueAsync(role.Description);

        // Verify assigned User is displayed
        locator = Page.GetByRole(AriaRole.Cell, new() { Name = login.Email });
        await Assertions.Expect(locator).ToHaveCountAsync(1);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' successfully deleted." })
            ).ToHaveCountAsync(1);
    }

    [Fact(DisplayName = "Delete button is disabled for system role")]
    public async Task Test005()
    {
        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = nameof(AppGuids.Role.DocumentManager) })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete Role", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = "System Roles may not be deleted!" })
            ).ToHaveCountAsync(1);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Button, new() { Name = "Delete" })
            ).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "Index page displays High Severity notification")]
    public async Task Test006()
    {
        var role = new ApplicationRole { Name = "Test06Role" };
        await WebAppFactory.EnsureRoleAsync(role);

        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete Role", title);

        await WebAppFactory.DeleteRoleAsync(role.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Assertions.Expect(
            Page.GetByRole(AriaRole.Heading, new() { Name = $"Error: Role '{role.Name}' was not found in the database." })
            ).ToHaveCountAsync(1);
    }

    [Fact(DisplayName = "Index page Create link disabled when Create claim missing")]
    public async Task Test007()
    {
        var role = new ApplicationRole
        {
            Name = "CalendarReader",
            Description = "Test Role"
        }.SetClaims(SysClaims.Role.DefinedClaims.Except([SysClaims.Role.Create]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test07User@company.com", "TestO7pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        var locator = Page.GetByRole(AriaRole.Link, new() { Name = "Create New" });
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "Index page Edit button disabled when Update claim missing")]
    public async Task Test008()
    {
        var role = new ApplicationRole
        {
            Name = "Test08Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.Role.DefinedClaims.Except([SysClaims.Role.Update]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test08User@company.com", "TestO8pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByLabel("Edit");
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "View button disabled on Index page when Read claim missing")]
    public async Task Test009()
    {
        var role = new ApplicationRole
        {
            Name = "Test09Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.Role.DefinedClaims.Except([SysClaims.Role.Read]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test09User@company.com", "TestO9pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByLabel("View");
        await Assertions.Expect(locator).ToBeDisabledAsync();
    }

    [Fact(DisplayName = "Delete button disabled on Index page when Delete claim missing")]
    public async Task Test010()
    {
        var role = new ApplicationRole
        {
            Name = "Test10Role",
            Description = "Test Role"
        }.SetClaims(SysClaims.Role.DefinedClaims.Except([SysClaims.Role.Delete]));
        await WebAppFactory.EnsureRoleAsync(role);

        var login = new Login("Test10User@company.com", "Test10pa$$");
        await WebAppFactory.EnsureUserAsync(login, role.Id);

        await LogUserInAsync(login, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        var locator = Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
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
