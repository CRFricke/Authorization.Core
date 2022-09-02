using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Linq;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Extensions
{
    public static class HtmlAssert
    {
        static readonly string[] ExpectedDuplicates = new[] { "btnCreate", "btnDelete", "btnSave" };

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
            var elements = document.QuerySelectorAll(selector);
            if (elements.Length > 1)
            {
                if (!ExpectedDuplicates.Contains(elements[0].Id))
                {
                    Assert.Single(elements);
                }
            }

            return Assert.IsAssignableFrom<IHtmlElement>(elements.First());
        }
    }
}
