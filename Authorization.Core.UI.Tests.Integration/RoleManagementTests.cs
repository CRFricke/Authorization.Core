using Authorization.Core.UI.Test.Web;
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

public partial class RoleManagementTests : PageTest, IClassFixture<PlaywrightTestFixture>, IAsyncLifetime
{
    internal RoleManagementTests(PlaywrightTestFixture fixture) : base(fixture.Browser)
    {
        Fixture = fixture;
        WebAppFactory = Fixture.WebAppFactory;
        Client = WebAppFactory.CreateClient(new() { BaseAddress = new(HostUri) });
    }

    public RoleManagementTests(PlaywrightTestFixture fixture, ITestOutputHelper outputHelper) : this(fixture)
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

    private async Task DeleteRoleAsync(string roleId)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dbRole = await dbContext.Roles.FindAsync(roleId);
        if (dbRole is not null)
        {
            dbContext.Roles.Remove(dbRole);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Roles
            .Where(r => r.Id == roleId)
            .Include(r => r.Claims)
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

    private async Task VerifyRoleExistsAsync(string roleName)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.True( await dbContext.Roles.AnyAsync(r => r.Name == roleName) );
    }

    #endregion


    [Fact(DisplayName = "Can create new role with claims")]
    public async Task RoleManagementTest01Async()
    {
        await LogUserInAsync(Logins.Administrator, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Create New" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Create Role", title);

        var roleName = "Test01Role";

        await Page.GetByLabel("Name").FillAsync(roleName);
        await Page.GetByLabel("Description").FillAsync("Test Role");
        await Page.GetByLabel("Search:").FillAsync("List");
        await Page.GetByRole(AriaRole.Row, new() { Name = "Bulletin.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Row, new() { Name = "Calendar.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Row, new() { Name = "Document.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Row, new() { Name = "News.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Row, new() { Name = "Role.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Row, new() { Name = "User.List" }).GetByRole(AriaRole.Checkbox).CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create" }).ClickAsync();

        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);
        var locator = Page.Locator("div .ac-notifications")
            .GetByRole(AriaRole.Heading, new() { Name = $"Role '{roleName}' successfully created." });
        Assert.Equal(1, await locator.CountAsync());

        await VerifyRoleExistsAsync(roleName);
    }

    [Fact(DisplayName = "Can display existing role")]
    public async void RoleManagementTest02Async()
    {
        var role = await GetRoleByIdAsync(SysUiGuids.Role.UserManager);
        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "View" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Details", title);

        var value = await Page.GetByLabel("Id").InputValueAsync();
        Assert.Equal(role.Id, value);
        value = await Page.GetByLabel("Name").InputValueAsync();
        Assert.Equal(role.Name, value);
        value = await Page.GetByLabel("Description").InputValueAsync();
        Assert.Equal(role.Description, value);

        var roleClaims = role.Claims
            .Select(r => r.ClaimValue!)
            .ToList();

        var rows = Page.GetByRole(AriaRole.Row);
        var count = await rows.CountAsync();
        Assert.True(count > 0);

        for (var ix = 1; ix < count; ix++)
        {
            value = await rows.Nth(ix).GetByRole(AriaRole.Cell).Last.InnerTextAsync();
            Assert.Contains(value, roleClaims);
        }
    }

    [Fact(DisplayName = "Can update existing role and claims")]
    public async void RoleManagementTest03Async()
    {
        var role = new ApplicationRole 
        {
            Name = "Test03Role", Description = "Can list and view news items." 
        }.SetClaims(AppClaims.News.List, AppClaims.News.Read);
        await Fixture.EnsureRoleAsync(role);

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
        var value = await locator.InputValueAsync();
        Assert.Equal(role.Name, value);
        role.Name = "Test03RoleX";
        await locator.FillAsync(role.Name);

        locator = Page.GetByLabel(nameof(role.Description));
        value = await locator.InputValueAsync();
        Assert.Equal(role.Description, value);
        role.Description = "Can create, delete, list, read, and update news items.";
        await locator.FillAsync(role.Description);

        await Page.GetByLabel("Search:").FillAsync("News");

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.News.List }).GetByRole(AriaRole.Checkbox);
        Assert.True(await locator.IsCheckedAsync());

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.News.Read }).GetByRole(AriaRole.Checkbox);
        Assert.True(await locator.IsCheckedAsync());

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.News.Create }).GetByRole(AriaRole.Checkbox);
        Assert.False(await locator.IsCheckedAsync());
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.News.Delete }).GetByRole(AriaRole.Checkbox);
        Assert.False(await locator.IsCheckedAsync());
        await locator.CheckAsync();

        locator = Page.GetByRole(AriaRole.Row, new() { Name = AppClaims.News.Update }).GetByRole(AriaRole.Checkbox);
        Assert.False(await locator.IsCheckedAsync());
        await locator.CheckAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);
        locator = Page.Locator("div .ac-notifications")
            .GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' was successfully updated." });
        Assert.NotNull(locator);

        role.SetClaims(AppClaims.News.DefinedClaims);

        var dbRole = await GetRoleByIdAsync(role.Id);
        Assert.NotNull(dbRole);
        Assert.Equal(role.Name, dbRole.Name);
        Assert.Equal(role.Description, dbRole.Description);
        Assert.Equal(
            role.Claims.Select(c => c.ClaimValue!).Order(), 
            dbRole.Claims.Select(c => c.ClaimValue!).Order()
            );
    }

    [Fact(DisplayName = "Can delete existing role")]
    public async void RoleManagementTest04Async()
    {
        var role = new ApplicationRole
        {
            Name = "Test04Role",
            Description = "Can create, delete, list, read, and update calendar items."
        }.SetClaims(AppClaims.Calendar.DefinedClaims);
        await Fixture.EnsureRoleAsync(role);

        var login = new Login("Test04User@company.com", "Test04pa$$");
        await Fixture.EnsureUserAsync(login, role.Name);

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
        Assert.Equal(role.Id, await locator.InputValueAsync());

        locator = Page.GetByRole(AriaRole.Textbox).Nth(1);
        Assert.Equal(role.Name, await locator.InputValueAsync());

        locator = Page.GetByRole(AriaRole.Textbox).Nth(2);
        Assert.Equal(role.Description, await locator.InputValueAsync());

        // Verify assigned User is displayed
        locator = Page.GetByRole(AriaRole.Cell, new() { Name = login.Email });
        Assert.Equal(1, await locator.CountAsync());

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        locator = Page.GetByRole(AriaRole.Heading, new() { Name = $"Role '{role.Name}' successfully deleted." });
        Assert.Equal(1, await locator.CountAsync());
    }

    [Fact(DisplayName = "Delete button is disabled for system role")]
    public async void RoleManagementTest05Async()
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

        var locator = Page.GetByRole(AriaRole.Heading, new() { Name = "System Roles may not be deleted!" });
        Assert.Equal(1, await locator.CountAsync());

        // This test fails intermittently in the CI/CD pipeline due to a race condition where 
        // the IsDisabled test on the 'Delete' button occurs before the button is actually
        // disabled in the GUI. Adding the WaitForAsync call to try to eliminate this.
        locator = Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First;
        Assert.True(await locator.IsDisabledAsync());
    }

    [Fact(DisplayName = "Index page displays High Severity notification")]
    public async void RoleManagementTest06Async()
    {
        var role = new ApplicationRole { Name = "Test06Role" };
        await Fixture.EnsureRoleAsync(role);

        await LogUserInAsync(Logins.RoleManager, "/Admin/Role");
        var title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        await Page.GetByRole(AriaRole.Row)
            .Filter(new() { HasText = role!.Name })
            .GetByRole(AriaRole.Link, new() { Name = "Delete" })
            .ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Delete Role", title);

        await DeleteRoleAsync(role.Id);

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).First.ClickAsync();
        title = await Page.TitleAsync();
        Assert.Contains("Role Management", title);

        var locator = Page.Locator("div .ac-notifications")
            .GetByRole(AriaRole.Heading, new() { Name = $"Error: Role '{role.Name}' was not found in the database." });
        Assert.Equal(1, await locator.CountAsync());
    }
}
