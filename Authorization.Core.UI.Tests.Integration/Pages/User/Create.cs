using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using Authorization.Core.UI.Tests.Integration.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.User
{
    public class Create : BasePage
    {
        internal const string Title = "Create User";

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
            var responseMessage = await client.GetAsync($"Admin/User/Create");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Create(client, document, context ?? new UIPageContext());
        }

        internal async Task<Index> ClickCreateButtonAsync(UserModel userModel)
        {
            return await ClickCreateButtonAsync(userModel, Array.Empty<string>());
        }

        internal async Task<Index> ClickCreateButtonAsync(UserModel userModel, params string[] roleNames)
        {
            var responseMessage = await Client.SendAsync(_createForm, _createButton, FillCreateForm(userModel, roleNames));
            var redirectUri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal(Index.Path, redirectUri.OriginalString);
            responseMessage = await Client.GetAsync(redirectUri);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Index.Title, document.Title);

            return new Index(Client, document, Context);
        }

        private static Dictionary<string, string> FillCreateForm(UserModel userModel, string[] roleNames)
        {
            return new Dictionary<string, string>()
            {
                ["Input_Email"] = userModel.Email,
                ["Input_Password"] = userModel.Password,
                ["Input_ConfirmPassword"] = userModel.Password,
                ["Input_GivenName"] = userModel.GivenName,
                ["Input_Surname"] = userModel.Surname,
                ["Input_PhoneNumber"] = userModel.PhoneNumber,
                ["Input_LockoutEnabled"] = userModel.LockoutEnabled.ToString(),
                ["hfRoleList"] = string.Join(',', roleNames)
            };
        }
    }
}
