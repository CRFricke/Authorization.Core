using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Collections.Generic;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration
{
    public class PageAccessTestsPart1 : IClassFixture<WebAppFactory>
    {
        public PageAccessTestsPart1(WebAppFactory webAppFactory)
        {
            WebAppFactory = webAppFactory;
        }

        private WebAppFactory WebAppFactory { get; }

        [Theory(DisplayName = "Can access Management page via friendly area name")]
        [MemberData(nameof(Test01Data))]
        public async void PageAccessTest01(string endpoint, string pageTitle, bool needsId)
        {
            if (needsId)
            {
                var id = (endpoint.Contains("Role"))
                    ? WebAppFactory.AppRoleId : WebAppFactory.AppUserId;
                endpoint += $"?id={id}";
            }

            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync(endpoint);

            var index = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(pageTitle, index.Title);
        }

        [Fact(DisplayName = "Can access customer page actually located in friendly name area")]
        public async void PageAccessTest02()
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync("/Admin/Calendar");

            var index = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains("Calendar Management", index.Title);
        }

        [Theory(DisplayName = "Can access Management pages in default area")]
        [MemberData(nameof(Test03Data))]
        public async void PageAccessTest03(string endpoint, string pageTitle, bool needsId)
        {
            if (needsId)
            {
                var id = (endpoint.Contains("Role"))
                    ? WebAppFactory.AppRoleId : WebAppFactory.AppUserId;
                endpoint += $"?id={id}";
            }

            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(client, "Admin@company.com", "Administrat0r!");
            var responseMessage = await client.GetAsync(endpoint);

            var index = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(pageTitle, index.Title);
        }

        [Theory(DisplayName = "Redirects to AccessDenied page without proper claim")]
        [MemberData(nameof(Test04Data))]
        public async void PageAccessTest04(string endpoint)
        {
            var client = WebAppFactory.CreateClient();

            await WebAppFactory.LoginExistingUserAsync(
                client, WebAppFactory.AppUserEmail, WebAppFactory.AppUserPassword
                );

            var responseMessage = await client.GetAsync(endpoint);
            var uri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal("/Identity/Account/AccessDenied", uri.LocalPath);
        }


        public static IEnumerable<object[]> Test01Data => new List<object[]>
        {
            new object[] { "/Admin/Role", "Role Management", false },
            new object[] { "/Admin/Role/Create", "Create Role", false },
            new object[] { "/Admin/Role/Delete", "Delete Role", true },
            new object[] { "/Admin/Role/Details", "Role Details", true },
            new object[] { "/Admin/Role/Edit", "Edit Role", true },
            new object[] { "/Admin/Role/Index", "Role Management", false },
            new object[] { "/Admin/User", "User Management", false },
            new object[] { "/Admin/User/Create", "Create User", false },
            new object[] { "/Admin/User/Delete", "Delete User", true },
            new object[] { "/Admin/User/Details", "User Details", true },
            new object[] { "/Admin/User/Edit", "Edit User", true },
            new object[] { "/Admin/User/Index", "User Management", false }
        };

        public static IEnumerable<object[]> Test03Data => new List<object[]>
        {
            new object[] { "/Authorization/Role", "Role Management", false },
            new object[] { "/Authorization/Role/Create", "Create Role", false },
            new object[] { "/Authorization/Role/Delete", "Delete Role", true },
            new object[] { "/Authorization/Role/Details", "Role Details", true },
            new object[] { "/Authorization/Role/Edit", "Edit Role", true },
            new object[] { "/Authorization/Role/Index", "Role Management", false },
            new object[] { "/Authorization/User", "User Management", false },
            new object[] { "/Authorization/User/Create", "Create User", false },
            new object[] { "/Authorization/User/Delete", "Delete User", true },
            new object[] { "/Authorization/User/Details", "User Details", true },
            new object[] { "/Authorization/User/Edit", "Edit User", true },
            new object[] { "/Authorization/User/Index", "User Management", false }
        };

        public static IEnumerable<object[]> Test04Data => new List<object[]>
        {
            new object[] { "/Admin/Role" },
            new object[] { "/Admin/Role/Create" },
            new object[] { "/Admin/Role/Delete" },
            new object[] { "/Admin/Role/Details" },
            new object[] { "/Admin/Role/Edit" },
            new object[] { "/Admin/Role/Index" },
            new object[] { "/Admin/User" },
            new object[] { "/Admin/User/Create" },
            new object[] { "/Admin/User/Delete" },
            new object[] { "/Admin/User/Details" },
            new object[] { "/Admin/User/Edit" },
            new object[] { "/Admin/User/Index" }
        };
    }
}
