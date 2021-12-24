using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Extensions
{
    public static class HtmlAssert
    {
        public static IHtmlFormElement HasForm(string selector, IParentNode document)
        {
            var form = Assert.Single(document.QuerySelectorAll(selector));
            return Assert.IsAssignableFrom<IHtmlFormElement>(form);
        }

        public static IHtmlAnchorElement HasLink(string selector, IHtmlDocument document)
        {
            var element = Assert.Single(document.QuerySelectorAll(selector));
            return Assert.IsAssignableFrom<IHtmlAnchorElement>(element);
        }

        public static IHtmlElement HasElement(string selector, IParentNode document)
        {
            var element = Assert.Single(document.QuerySelectorAll(selector));
            return Assert.IsAssignableFrom<IHtmlElement>(element);
        }
    }
}
