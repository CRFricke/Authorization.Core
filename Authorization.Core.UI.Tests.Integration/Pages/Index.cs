using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages
{
    public class Index : BasePage
    {
        public const string Path = "/";
        public const string Title = "Home page";

        private readonly IHtmlAnchorElement _loginLink;

        public Index(
            HttpClient client,
            IHtmlDocument document,
            UIPageContext context)
            : base(client, document, context)
        {
            if (!Context.UserAuthenticated)
            {
                _loginLink = HtmlAssert.HasLink("a[href*='Login']", Document);
            }
        }

        public static async Task<Index> CreateAsync(HttpClient client, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync("/");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Index(client, document, context ?? new UIPageContext());
        }

        public async Task<Login> ClickLoginLinkAsync()
        {
            Assert.False(Context.UserAuthenticated);

            var responseMessage = await Client.GetAsync(_loginLink.Href);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Login.Title, document.Title);

            return new Login(Client, document, Context);
        }
    }
}
