using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages
{
    public class Login : BasePage
    {
        internal const string Title = "Log in";

        private readonly IHtmlFormElement _loginForm;
        private readonly IHtmlElement _loginButton;

        public Login(
            HttpClient client,
            IHtmlDocument login,
            UIPageContext context)
            : base(client, login, context)
        {
            _loginForm = HtmlAssert.HasForm("#account", login);
            _loginButton = HtmlAssert.HasElement("#login-submit", login);
        }

        public static async Task<Login> CreateAsync(HttpClient client, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync("/Identity/Account/Login");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Login(client, document, context ?? new UIPageContext());
        }

        public async Task<Index> LoginValidUserAsync(string userName, string password)
        {
            var responseMessage = await SendLoginForm(userName, password);

            var redirectUri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal(Index.Path, redirectUri.OriginalString);
            responseMessage = await Client.GetAsync(redirectUri);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Index.Title, document.Title);

            return new Index(Client, document, Context.WithAuthenticatedUser().WithPasswordLogin());
        }

        private async Task<HttpResponseMessage> SendLoginForm(string userName, string password)
        {
            return await Client.SendAsync(_loginForm, _loginButton, new Dictionary<string, string>()
            {
                ["Input_Email"] = userName,
                ["Input_Password"] = password
            });
        }
    }
}
