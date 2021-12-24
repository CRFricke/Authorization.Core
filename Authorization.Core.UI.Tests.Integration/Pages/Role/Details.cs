using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using System.Linq;

namespace Authorization.Core.UI.Tests.Integration.Pages.Role
{
    public class Details : BasePage
    {
        internal const string Title = "Role Details";

        public Details(
            HttpClient client,
            IHtmlDocument document,
            UIPageContext context)
            : base(client, document, context)
        {
            InitProperties();
            InitClaims();
        }


        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public List<string> Claims { get; } = new List<string>();


        private void InitProperties()
        {
            var ddElements = Document.QuerySelectorAll("dd");
            Assert.Equal(3, ddElements.Length);

            Id = ddElements[0].TextContent.Trim();
            Name = ddElements[1].TextContent.Trim();
            Description = ddElements[2].TextContent.Trim();
        }

        private void InitClaims()
        {
            var trElements = Document.QuerySelectorAll("tr :checked");
            Claims.AddRange(
                from trElement in trElements
                select trElement.ParentElement.NextElementSibling.TextContent.Trim()
                );
        }
    }
}
