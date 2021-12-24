using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.User
{
    public class Details : BasePage
    {
        internal const string Title = "User Details";

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

        public string Email { get; private set; }

        public string GivenName { get; private set; }

        public string Surname { get; private set; }

        public string PhoneNumber { get; private set; }

        public DateTimeOffset? LockoutEnd { get; private set; }

        public int AccessFailedCount { get; private set; }

        public bool EmailConfirmed { get; private set; }

        public bool PhoneNumberConfirmed { get; private set; }

        public bool LockoutEnabled { get; private set; }

        public List<string> Roles { get; } = new List<string>();


        private void InitProperties()
        {
            var ddElements = Document.QuerySelectorAll("dd");
            Assert.Equal(10, ddElements.Length);

            Id = ddElements[0].TextContent.Trim();
            Email = ddElements[1].TextContent.Trim();
            GivenName = ddElements[2].TextContent.Trim();
            Surname = ddElements[3].TextContent.Trim();
            PhoneNumber = ddElements[4].TextContent.Trim();

            if (!string.IsNullOrWhiteSpace(ddElements[5].TextContent))
            {
                Assert.True(DateTimeOffset.TryParse(ddElements[5].TextContent.Trim(), out DateTimeOffset lockoutEnd));
                LockoutEnd = lockoutEnd;
            }

            Assert.True(int.TryParse(ddElements[6].TextContent.Trim(), out int accessFailedCount));
            AccessFailedCount = accessFailedCount;

            var checkBox = Assert.IsAssignableFrom<IHtmlInputElement>(ddElements[7].FirstElementChild);
            EmailConfirmed = checkBox.IsChecked;

            checkBox = Assert.IsAssignableFrom<IHtmlInputElement>(ddElements[8].FirstElementChild);
            PhoneNumberConfirmed = checkBox.IsChecked;

            checkBox = Assert.IsAssignableFrom<IHtmlInputElement>(ddElements[9].FirstElementChild);
            LockoutEnabled = checkBox.IsChecked;
        }

        private void InitClaims()
        {
            var trElements = Document.QuerySelectorAll("tr :checked");
            foreach (var trElement in trElements)
            {
                Roles.Add(
                    trElement.ParentElement.NextElementSibling.TextContent.Trim()
                    );
            }
        }
    }
}
