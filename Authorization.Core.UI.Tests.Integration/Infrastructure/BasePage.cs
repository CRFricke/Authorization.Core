using AngleSharp.Html.Dom;
using System.Net.Http;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    public abstract class BasePage
    {
        public BasePage(HttpClient client, IHtmlDocument document, UIPageContext context)
        {
            Client = client;
            Document = document;
            Context = context;
        }

        public HttpClient Client { get; }

        public IHtmlDocument Document { get; }

        public UIPageContext Context { get; }
    }
}
