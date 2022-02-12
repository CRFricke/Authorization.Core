using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;
using System.Linq;
using AngleSharp.Dom;

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
            Id = Assert.IsAssignableFrom<IHtmlInputElement>(
                Document.QuerySelectorAll("#RoleModel_Id").SingleOrDefault()
                )?.Value.Trim();
            Name = Assert.IsAssignableFrom<IHtmlInputElement>(
                Document.QuerySelectorAll("#RoleModel_Name").SingleOrDefault()
                )?.Value.Trim();
            Description = Assert.IsAssignableFrom<IHtmlTextAreaElement>(
               Document.QuerySelectorAll("#RoleModel_Description").SingleOrDefault()
               )?.Value.Trim();
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
