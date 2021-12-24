using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.Role
{
    public class Delete : BasePage
    {
        internal const string Title = "Delete Role";

        private readonly IHtmlFormElement _formDelete;
        private readonly IHtmlElement _btnDelete;

        public Delete(
            HttpClient client,
            IHtmlDocument document,
            UIPageContext context)
            : base(client, document, context)
        {
            _formDelete = HtmlAssert.HasForm("#formDelete", document);
            _btnDelete = HtmlAssert.HasElement("#btnDelete", document);
        }

        internal IHtmlElement DeleteButton { get => _btnDelete; }


        public static async Task<Delete> CreateAsync(HttpClient client, string roleId, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync($"Admin/Role/Delete?id={roleId}");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Delete(client, document, context ?? new UIPageContext());
        }

        internal async Task<Index> ClickDeleteButtonAsync()
        {
            var responseMessage = await Client.SendAsync(_formDelete, _btnDelete, new Dictionary<string, string>());
            var redirectUri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal(Index.Path, redirectUri.OriginalString);
            responseMessage = await Client.GetAsync(redirectUri);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Index.Title, document.Title);

            return new Index(Client, document, Context);
        }

        internal async Task<Delete> ClickDeleteButtonExpectingErrorAsync()
        {
            var responseMessage = await Client.SendAsync(_formDelete, _btnDelete, new Dictionary<string, string>());
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Delete(Client, document, Context);
        }

        internal string GetValidationSummaryText()
            => Document.QuerySelectorAll(".validation-summary-errors").FirstOrDefault()?.TextContent;
    }
}
