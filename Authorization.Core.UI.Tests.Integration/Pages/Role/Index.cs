using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.Role
{
    public class Index : BasePage
    {
        internal const string Path = "/Admin/Role/Index";
        internal const string Title = "Role Management";

        private readonly IHtmlAnchorElement _createNewLink;

        public Index(
            HttpClient client,
            IHtmlDocument document,
            UIPageContext context)
            : base(client, document, context)
        {
            _createNewLink = HtmlAssert.HasLink("a[href*='Create']", document);
        }

        public static async Task<Index> CreateAsync(HttpClient client, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync("/Admin/Role");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Index(client, document, context ?? new UIPageContext());
        }

        internal async Task<Create> ClickCreateNewLinkAsync()
        {
            var responseMessage = await Client.GetAsync(_createNewLink.Href);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Create.Title, document.Title);

            return new Create(Client, document, Context);
        }

        internal async Task<Details> ClickDetailsLinkAsync(string id)
        {
            var anchorElement = HtmlAssert.HasLink($"a[href*='Details?id={id}']", Document);
            var responseMessage = await Client.GetAsync(anchorElement.Href);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Details.Title, document.Title);

            return new Details(Client, document, Context);
        }

        internal async Task<Edit> ClickEditLinkAsync(string id)
        {
            var anchorElement = HtmlAssert.HasLink($"a[href*='Edit?id={id}']", Document);
            var responseMessage = await Client.GetAsync(anchorElement.Href);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Edit.Title, document.Title);

            return new Edit(Client, document, Context);
        }

        internal async Task<Delete> ClickDeleteLinkAsync(string id)
        {
            var anchorElement = HtmlAssert.HasLink($"a[href*='Delete?id={id}']", Document);
            var responseMessage = await Client.GetAsync(anchorElement.Href);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Delete.Title, document.Title);

            return new Delete(Client, document, Context);
        }

        internal string GetNotificationErrorText()
            => Document.QuerySelectorAll("h6.text-danger").FirstOrDefault().TextContent;

        internal string GetNotificationSuccessText()
            => Document.QuerySelectorAll("h6.text-success").FirstOrDefault().TextContent;
    }
}
