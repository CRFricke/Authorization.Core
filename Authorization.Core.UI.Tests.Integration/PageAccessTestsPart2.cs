using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using CRFricke.Authorization.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration
{
    public class PageAccessTestsPart2 : IClassFixture<WebAppFactory>
    {
        public PageAccessTestsPart2(WebAppFactory webAppFactory)
        {
            WebAppFactory = webAppFactory;
        }

        private WebAppFactory WebAppFactory { get; }

        [Theory(DisplayName = "Can access management page with proper claim")]
        [MemberData(nameof(Test01Data))]
        public async void PageAccessTest01(string endpoint, string claim, bool needsId)
        {
            var authManager = WebAppFactory.Services.GetRequiredService<IAuthorizationManager>();
            var dbContxt = WebAppFactory.Services.GetRequiredService<ApplicationDbContext>();

            var role = dbContxt.Roles.Find(WebAppFactory.AppRoleId);
            role.SetClaims(new[] { claim });
            dbContxt.Roles.Update(role);
            dbContxt.SaveChanges();

            authManager.RefreshRole(role.Id);

            if (needsId)
            {
                var id = (claim.StartsWith("Role"))
                    ? WebAppFactory.AppRoleId : WebAppFactory.AppUserId;
                endpoint += $"?id={id}";
            }

            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(
                client, WebAppFactory.AppUserEmail, WebAppFactory.AppUserPassword
                );

            var responseMessage = await client.GetAsync(endpoint);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);
        }

        public static IEnumerable<object[]> Test01Data => new List<object[]>
        {
            new object[] { "/Admin/Role", SysClaims.Role.List, false },
            new object[] { "/Admin/Role/Create", SysClaims.Role.Create, false },
            new object[] { "/Admin/Role/Delete", SysClaims.Role.Delete, true },
            new object[] { "/Admin/Role/Details", SysClaims.Role.Read, true },
            new object[] { "/Admin/Role/Edit", SysClaims.Role.Update, true },
            new object[] { "/Admin/Role/Index", SysClaims.Role.List, false },
            new object[] { "/Admin/User", SysClaims.User.List, false },
            new object[] { "/Admin/User/Create", SysClaims.User.Create, false },
            new object[] { "/Admin/User/Delete", SysClaims.User.Delete, true },
            new object[] { "/Admin/User/Details", SysClaims.User.Read, true },
            new object[] { "/Admin/User/Edit", SysClaims.User.Update, true },
            new object[] { "/Admin/User/Index", SysClaims.User.List, false }
        };
    }
}
