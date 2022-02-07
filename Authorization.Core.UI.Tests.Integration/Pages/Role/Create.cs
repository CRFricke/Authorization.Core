using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using Authorization.Core.UI.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.Role
{
    public class Create : BasePage
    {
        internal const string Title = "Create Role";

        private readonly IHtmlFormElement _createForm;
        private readonly IHtmlElement _createButton;

        public Create(
            HttpClient client,
            IHtmlDocument document,
            UIPageContext context)
            : base(client, document, context)
        {
            _createForm = HtmlAssert.HasForm("#formCreate", document);
            _createButton = HtmlAssert.HasElement("#btnCreate", document);
        }

        public static async Task<Create> CreateAsync(HttpClient client, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync($"Admin/Role/Create");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Create(client, document, context ?? new UIPageContext());
        }

        internal async Task<Index> ClickCreateButtonAsync(RoleModel roleModel)
        {
            return await ClickCreateButtonAsync(roleModel, Array.Empty<string>());
        }

        internal async Task<Index> ClickCreateButtonAsync(RoleModel roleModel, params string[] claimValues)
        {
            var responseMessage = await Client.SendAsync(_createForm, _createButton, FillCreateForm(roleModel, claimValues));
            var redirectUri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal(Index.Path, redirectUri.OriginalString);
            responseMessage = await Client.GetAsync(redirectUri);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Index.Title, document.Title);

            return new Index(Client, document, Context);
        }

        private static Dictionary<string, string> FillCreateForm(RoleModel roleModel, string[] claimValues)
        {
            return new Dictionary<string, string>()
            {
                ["RoleModel_Name"] = roleModel.Name,
                ["RoleModel.Description"] = roleModel.Description,
                ["hfClaimList"] = string.Join(',', claimValues)
            };
        }
    }
}