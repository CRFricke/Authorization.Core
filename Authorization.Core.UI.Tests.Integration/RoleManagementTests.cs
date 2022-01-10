using AngleSharp.Dom;
using Authorization.Core.UI.Test.Web;
using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using Authorization.Core.UI.Tests.Integration.Models;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration
{
    public class RoleManagementTests : IClassFixture<WebAppFactory>
    {
        public RoleManagementTests(WebAppFactory webAppFactory)
        {
            WebAppFactory = webAppFactory;
        }

        public WebAppFactory WebAppFactory { get; }

        private readonly RoleModel ListerRoleModel = new RoleModel
        {
            Name = "Lister",
            Description = "Can list things"
        };

        private readonly string[] ListerRoleClaims = new string[]
        {
                SysClaims.Role.List, SysClaims.User.List, AppClaims.Bulletin.List, AppClaims.Calendar.List, AppClaims.Document.List, AppClaims.News.List
        };


        [Fact(DisplayName = "Can create new role with claims")]
        public async void RoleManagementTest01Async()
        {
            var client = WebAppFactory.CreateClient();
            var requiredClaims = new[]
            {
                SysClaims.Role.List, SysClaims.Role.Create,
                SysClaims.User.List, AppClaims.Bulletin.List, AppClaims.Calendar.List, AppClaims.Document.List, AppClaims.News.List
            };
            await SetupTestEnvironmentAsync(client, requiredClaims);

            var index = await Pages.Role.Index.CreateAsync(client);
            var create = await index.ClickCreateNewLinkAsync();
            await create.ClickCreateButtonAsync(ListerRoleModel, ListerRoleClaims);

            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
            var role = await dbContext.Roles
                .Include(ar => ar.Claims)
                .Where(ar => ar.Name == ListerRoleModel.Name)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            Assert.NotNull(role);
            Assert.Equal(ListerRoleModel.Description, role.Description);
            Assert.Equal(ListerRoleClaims.Length, role.Claims.Count);
        }

        [Fact(DisplayName = "Can display existing role")]
        public async void RoleManagementTest02()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client, SysClaims.Role.List, SysClaims.Role.Read);
            var role = EnsureListerRoleExists();

            var index = await Pages.Role.Index.CreateAsync(client);
            var details = await index.ClickDetailsLinkAsync(role.Id);

            Assert.Equal(role.Description, details.Description);
            Assert.Equal(role.Id, details.Id);
            Assert.Equal(role.Name, details.Name);

            Assert.Equal(role.Claims.Count, details.Claims.Count);
            foreach (var claim in role.Claims)
            {
                Assert.Contains(claim.ClaimValue, details.Claims);
            }
        }

        [Fact(DisplayName = "Can update existing role and claims")]
        public async void RoleManagementTest03()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client,
                SysClaims.Role.List, SysClaims.Role.Read, SysClaims.Role.Update, SysClaims.Role.UpdateClaims
                );
            var role = EnsureListerRoleExists();

            var tracker = new RoleTracker(role)
                .SetValue(nameof(role.Description), role.Description += ": Updated!")
                .SetClaims(SysClaims.Role.List, SysClaims.Role.Read);

            var index = await Pages.Role.Index.CreateAsync(client);
            var edit = await index.ClickEditLinkAsync(role.Id);
            edit.UpdateProperties(tracker.GetUpdates()).SetClaims(tracker.GetCurrentClaims());
            await edit.ClickSaveButtonAsync();

            role = EnsureListerRoleExists();

            Assert.Equal(tracker.Id, role.Id);
            Assert.Equal(tracker.Name, role.Name);
            Assert.Equal(tracker.Description, role.Description);

            Assert.Equal(tracker.GetCurrentClaims().Length, role.Claims.Count);
            foreach (var claim in role.Claims)
            {
                Assert.Contains(claim.ClaimValue, tracker.GetCurrentClaims());
            }
        }

        [Fact(DisplayName = "Can delete existing role")]
        public async void RoleManagementTest04()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client, SysClaims.Role.List, SysClaims.Role.Delete);
            var role = EnsureListerRoleExists();

            var index = await Pages.Role.Index.CreateAsync(client);
            var delete = await index.ClickDeleteLinkAsync(role.Id);
            await delete.ClickDeleteButtonAsync();

            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
            Assert.False(
                 dbContext.Roles.Any(ar => ar.Id == role.Id)
                );
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Delete page")]
        public async void RoleManagementTest05()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/Role/Delete");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Delete page")]
        public async void RoleManagementTest06()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/Role/Delete?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Details page")]
        public async void RoleManagementTest07()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/Role/Details");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Details page")]
        public async void RoleManagementTest08()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/Role/Details?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Edit page")]
        public async void RoleManagementTest09()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/Role/Edit");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Edit page")]
        public async void RoleManagementTest10()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/Role/Edit?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Delete page disables Delete button for system Role")]
        public async void RoleManagementTest11()
        {
            var roleId = SysUiGuids.Role.UserManager;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var delete = await Pages.Role.Delete.CreateAsync(client, roleId);
            Assert.True(delete.DeleteButton.IsDisabled());
        }

        [Fact(DisplayName = "Delete page displays authorization failure message")]
        public async void RoleManagementTest12()
        {
            var roleId = SysUiGuids.Role.UserManager;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var delete = await Pages.Role.Delete.CreateAsync(client, roleId);
            delete = await delete.ClickDeleteButtonExpectingErrorAsync();
            Assert.Contains("Can not delete Role", delete.GetValidationSummaryText());
        }

        [Fact(DisplayName = "Edit page displays authorization failure message")]
        public async void RoleManagementTest13()
        {
            var roleId = SysUiGuids.Role.UserManager;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = (await Pages.Role.Edit.CreateAsync(client, roleId)).SetClaims(new string[] { });
            edit = await edit.ClickSaveButtonExpectingErrorAsync();
            Assert.Contains("Can not update Role", edit.GetValidationSummaryText());
        }

        [Fact(DisplayName = "Index page displays High Severity notification")]
        public async void RoleManagementTest14()
        {
            var userId = SysGuids.Role.Administrator;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = (await Pages.Role.Edit.CreateAsync(client, userId))
                .UpdateProperties(new Dictionary<string, string> { { nameof(AuthUiRole.Id), Guid.Empty.ToString() } });
            var index = await edit.ClickSaveButtonAsync();

            Assert.Contains("not found in the database", index.GetNotificationErrorText());
        }

        [Fact(DisplayName = "Index page displays Normal Severity notification")]
        public async void RoleManagementTest15()
        {
            var name = "TestRole2";
            var roleModel = new RoleModel { Name = name };
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = await Pages.Role.Create.CreateAsync(client);
            var index = await edit.ClickCreateButtonAsync(roleModel);

            Assert.Contains($"'{name}' successfully created", index.GetNotificationSuccessText());
        }


        private async Task SetupTestEnvironmentAsync(HttpClient client, params string[] requiredClaims)
        {
            var authManager = WebAppFactory.Services.GetRequiredService<IAuthorizationManager>();
            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();

            var role = dbContext.Roles.Find(WebAppFactory.AppRoleId);
            Assert.NotNull(role);
            role.SetClaims(requiredClaims);
            dbContext.SaveChanges();

            authManager.RefreshRole(role.Id);

            await WebAppFactory.LoginExistingUserAsync(client, WebAppFactory.AppUserEmail, WebAppFactory.AppUserPassword);
        }

        private AuthUiRole EnsureListerRoleExists()
        {
            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
            var role = dbContext.Roles
                .Include(ar => ar.Claims)
                .Where(ar => ar.Name == ListerRoleModel.Name)
                .AsNoTracking()
                .SingleOrDefault();

            if (role == null)
            {
                role = ListerRoleModel.ToRole().SetClaims(ListerRoleClaims);

                dbContext.Roles.Add(role);
                dbContext.SaveChanges();
            }

            return role;
        }
    }
}
