using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var fcElements = Document.QuerySelectorAll(".form-control");

            Id = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_Id")
                )?.Value.Trim();
            Email = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_Email")
                )?.Value.Trim();
            GivenName = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_GivenName")
                )?.Value.Trim();
            Surname = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_Surname")
                )?.Value.Trim();
            PhoneNumber = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_PhoneNumber")
                )?.Value.Trim();

            var hie = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_LockoutEnd")
                );
            if (!string.IsNullOrWhiteSpace(hie?.Value))
            {
                Assert.True(DateTimeOffset.TryParse(hie.Value.Trim(), out DateTimeOffset lockoutEnd));
                LockoutEnd = lockoutEnd;
            }

            hie = Assert.IsAssignableFrom<IHtmlInputElement>(
                fcElements.SingleOrDefault(e => e.Id == "UserModel_AccessFailedCount")
                );
            if (!string.IsNullOrWhiteSpace(hie?.Value))
            {
                Assert.True(int.TryParse(hie.Value.Trim(), out int accessFailedCount));
                AccessFailedCount = accessFailedCount;
            }

            var fciElements = Document.QuerySelectorAll(".form-check-input");

            EmailConfirmed = Assert.IsAssignableFrom<IHtmlInputElement>(
                fciElements.SingleOrDefault(e => e.Id == "UserModel_EmailConfirmed")
                )?.IsChecked ?? false;
            PhoneNumberConfirmed = Assert.IsAssignableFrom<IHtmlInputElement>(
                fciElements.SingleOrDefault(e => e.Id == "UserModel_PhoneNumberConfirmed")
                )?.IsChecked ?? false;
            LockoutEnabled = Assert.IsAssignableFrom<IHtmlInputElement>(
                fciElements.SingleOrDefault(e => e.Id == "UserModel_LockoutEnabled")
                )?.IsChecked ?? false;
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
