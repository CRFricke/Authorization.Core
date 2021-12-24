using AngleSharp.Dom;
using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using Authorization.Core.UI.Tests.Integration.Models;
using CRFricke.Authorization.Core;
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
    public class UserManagementTests : IClassFixture<WebAppFactory>
    {
        public UserManagementTests(WebAppFactory webAppFactory)
        {
            WebAppFactory = webAppFactory;
        }

        public WebAppFactory WebAppFactory { get; }

        private readonly UserModel TesterUserModel = new UserModel
        {
            Id = Guid.NewGuid().ToString(),
            Email = "TestUser@company.com",
            Password = "Passw0rd!",
            GivenName = "Test",
            Surname = "User",
            PhoneNumber = "123-456-7890",
            LockoutEnabled = true
        };

        private readonly string[] TesterUserClaims = new string[]
        {
            nameof(SysGuids.Role.RoleManager), nameof(SysGuids.Role.UserManager)
        };


        [Fact(DisplayName = "Can create new user with roles")]
        public async void UserManagementTest01()
        {
            var client = WebAppFactory.CreateClient();
            var requiredClaims = new[]
            {
                SysClaims.User.List, SysClaims.User.Create,
                SysClaims.User.Delete, SysClaims.User.Read, SysClaims.User.Update, SysClaims.User.UpdateClaims,
                SysClaims.Role.List, SysClaims.Role.Create, SysClaims.Role.Delete, SysClaims.Role.Read, SysClaims.Role.Update, SysClaims.Role.UpdateClaims
            };
            await SetupTestEnvironmentAsync(client, requiredClaims);

            var index = await Pages.User.Index.CreateAsync(client);
            var create = await index.ClickCreateNewLinkAsync();
            await create.ClickCreateButtonAsync(TesterUserModel, TesterUserClaims);

            var user = EnsureTesterUserExists();

            Assert.NotNull(user);
            Assert.Equal(TesterUserModel.GivenName, user.GivenName);
            Assert.Equal(TesterUserModel.Surname, user.Surname);
            Assert.Equal(TesterUserClaims.Length, user.Claims.Count);
        }

        [Fact(DisplayName = "Can display existing user")]
        public async void UserManagementTest02()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client, SysClaims.User.List, SysClaims.User.Read);
            var user = EnsureTesterUserExists();

            var index = await Pages.User.Index.CreateAsync(client);
            var details = await index.ClickDetailsLinkAsync(user.Id);

            Assert.Equal(user.AccessFailedCount, details.AccessFailedCount);
            Assert.Equal(user.Email, details.Email);
            Assert.Equal(user.EmailConfirmed, details.EmailConfirmed);
            Assert.Equal(user.GivenName, details.GivenName);
            Assert.Equal(user.Id, details.Id);
            Assert.Equal(user.LockoutEnabled, details.LockoutEnabled);
            Assert.Equal(user.LockoutEnd, details.LockoutEnd);
            Assert.Equal(user.PhoneNumber, details.PhoneNumber);
            Assert.Equal(user.PhoneNumberConfirmed, details.PhoneNumberConfirmed);
            Assert.Equal(user.Surname, details.Surname);

            foreach (var claim in user.Claims)
            {
                Assert.Contains(claim.ClaimValue, details.Roles);
            }
        }

        [Fact(DisplayName = "Can update existing user and claims")]
        public async void UserManagementTest03()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client,
                SysClaims.User.List, SysClaims.User.Read, SysClaims.User.Update, SysClaims.User.UpdateClaims
                );
            var user = EnsureTesterUserExists();
            var tracker = new UserTracker(user)
                .SetValue(nameof(user.GivenName), "Ferd")
                .SetValue(nameof(user.Surname), "Burfel")
                .SetValue(nameof(user.LockoutEnd), new DateTimeOffset?(DateTime.Now.AddHours(3)))
                .SetClaims(nameof(SysGuids.Role.RoleManager), nameof(SysGuids.Role.UserManager));

            var index = await Pages.User.Index.CreateAsync(client);
            var edit = await index.ClickEditLinkAsync(user.Id);
            edit.UpdateProperties(tracker.GetUpdates()).SetClaims(tracker.GetCurrentClaims());
            await edit.ClickSaveButtonAsync();

            user = EnsureTesterUserExists();

            Assert.Equal(tracker.AccessFailedCount, user.AccessFailedCount);
            Assert.Equal(tracker.Email, user.Email);
            Assert.Equal(tracker.EmailConfirmed, user.EmailConfirmed);
            Assert.Equal(tracker.GivenName, user.GivenName);
            Assert.Equal(tracker.Id, user.Id);
            Assert.Equal(tracker.LockoutEnabled, user.LockoutEnabled);
            Assert.Equal(tracker.LockoutEnd.TrimMilliseconds(), user.LockoutEnd);
            Assert.Equal(tracker.PhoneNumber, user.PhoneNumber);
            Assert.Equal(tracker.PhoneNumberConfirmed, user.PhoneNumberConfirmed);
            Assert.Equal(tracker.Surname, user.Surname);
            Assert.Equal(tracker.TwoFactorEnabled, user.TwoFactorEnabled);
            Assert.Equal(tracker.UserName, user.UserName);

            Assert.Equal(tracker.GetCurrentClaims().Length, user.Claims.Count);
            foreach (var claim in user.Claims)
            {
                Assert.Contains(claim.ClaimValue, tracker.GetCurrentClaims());
            }
        }

        [Fact(DisplayName = "Can delete existing user")]
        public async void UserManagementTest04()
        {
            var client = WebAppFactory.CreateClient();
            await SetupTestEnvironmentAsync(client, SysClaims.User.List, SysClaims.User.Delete);
            var user = EnsureTesterUserExists();

            var index = await Pages.User.Index.CreateAsync(client);
            var delete = await index.ClickDeleteLinkAsync(user.Id);
            await delete.ClickDeleteButtonAsync();

            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
            Assert.False(
                await dbContext.Users.AnyAsync(au => au.Id == user.Id)
                );
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Delete page")]
        public async void UserManagementTest05()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/User/Delete");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Delete page")]
        public async void UserManagementTest06()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/User/Delete?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Details page")]
        public async void UserManagementTest07()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/User/Details");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Details page")]
        public async void UserManagementTest08()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/User/Details?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing null ID to Edit page")]
        public async void UserManagementTest09()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("Admin/User/Edit");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Receive NotFound passing non-existing ID to Edit page")]
        public async void UserManagementTest10()
        {
            var id = Guid.Empty.ToString();
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync($"Admin/User/Edit?id={id}");

            Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
        }

        [Fact(DisplayName = "Delete page disables Delete button for system User")]
        public async void UserManagementTest11()
        {
            var roleId = SysGuids.User.Administrator;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var delete = await Pages.User.Delete.CreateAsync(client, roleId);

            Assert.True(delete.DeleteButton.IsDisabled());
        }

        [Fact(DisplayName = "Delete page displays authorization failure message")]
        public async void RoleManagementTest12()
        {
            var roleId = SysGuids.User.Administrator;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var delete = await Pages.User.Delete.CreateAsync(client, roleId);
            delete = await delete.ClickDeleteButtonExpectingErrorAsync();

            Assert.Contains("Can not delete User", delete.GetValidationSummaryText());
        }

        [Fact(DisplayName = "Edit page displays authorization failure message")]
        public async void UserManagementTest13()
        {
            var userId = SysGuids.User.Administrator;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = (await Pages.User.Edit.CreateAsync(client, userId))
                .SetClaims(new [] { nameof(SysGuids.Role.UserManager) });
            edit = await edit.ClickSaveButtonExpectingErrorAsync();

            Assert.Contains("Can not update User", edit.GetValidationSummaryText());
        }

        [Fact(DisplayName = "Index page displays High Severity notification")]
        public async void UserManagementTest14()
        {
            var userId = SysGuids.User.Administrator;
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = (await Pages.User.Edit.CreateAsync(client, userId))
                .UpdateProperties(new Dictionary<string, string> { { nameof(AppUser.Id), Guid.Empty.ToString() } });
            var index = await edit.ClickSaveButtonAsync();

            Assert.Contains("not found in the database", index.GetNotificationErrorText());
        }

        [Fact(DisplayName = "Index page displays Normal Severity notification")]
        public async void UserManagementTest15()
        {
            var email = "TestUser2@company.com";
            var userModel = new UserModel { Email = email, Password = "P@ssw0rd!", };
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var edit = await Pages.User.Create.CreateAsync(client);
            var index = await edit.ClickCreateButtonAsync(userModel);

            Assert.Contains($"'{email}' successfully created", index.GetNotificationSuccessText());
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

        private AppUser EnsureTesterUserExists()
        {
            var dbContext = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users
                .Include(au => au.Claims)
                .Where(au => au.Id == TesterUserModel.Id)
                .AsNoTracking()
                .SingleOrDefault();

            if (user == null)
            {
                user = TesterUserModel.ToUser().SetClaims(TesterUserClaims);

                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }

            return user;
        }
    }
}
