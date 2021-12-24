using AngleSharp.Html.Dom;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests.Integration.Pages.Role
{
    public class Edit : BasePage
    {
        private const string IdPrefix = "RoleModel_";
        internal const string Title = "Edit Role";

        private readonly IHtmlFormElement _formEdit;
        private readonly IHtmlElement _btnSave;

        public Edit(
        HttpClient client,
        IHtmlDocument document,
        UIPageContext context)
        : base(client, document, context)
        {
            _formEdit = HtmlAssert.HasForm("#formEdit", document);
            _btnSave = HtmlAssert.HasElement("#btnSave", document);

            InitProperties();
            InitClaims();
        }


        private Dictionary<string, string> PropertyValues { get; } = new Dictionary<string, string>();

        public List<string> Claims { get; } = new List<string>();


        public static async Task<Edit> CreateAsync(HttpClient client, string roleId, UIPageContext context = null)
        {
            var responseMessage = await client.GetAsync($"Admin/Role/Edit?id={roleId}");
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Edit(client, document, context ?? new UIPageContext());
        }

        internal async Task<Index> ClickSaveButtonAsync()
        {
            var responseMessage = await Client.SendAsync(_formEdit, _btnSave, UpdateFormValues());
            var redirectUri = ResponseAssert.IsRedirect(responseMessage);
            Assert.Equal(Index.Path, redirectUri.OriginalString);
            responseMessage = await Client.GetAsync(redirectUri);
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Index.Title, document.Title);

            return new Index(Client, document, Context);
        }

        internal async Task<Edit> ClickSaveButtonExpectingErrorAsync()
        {
            var responseMessage = await Client.SendAsync(_formEdit, _btnSave, UpdateFormValues());
            var document = await ResponseAssert.IsHtmlDocumentAsync(responseMessage);
            Assert.Contains(Title, document.Title);

            return new Edit(Client, document, Context);
        }

        internal string GetValidationSummaryText()
            => Document.QuerySelectorAll(".validation-summary-errors").FirstOrDefault()?.TextContent;

        private IDictionary<string,string> UpdateFormValues()
        {
            var formValues = new Dictionary<string, string>();

            foreach (var kvp in PropertyValues)
            {
                formValues.Add($"{IdPrefix}{kvp.Key}", kvp.Value);
            }

            formValues.Add("hfClaimList", string.Join(',', Claims));

            return formValues;
        }

        private void InitClaims()
        {
            var trElements = Document.QuerySelectorAll("tr :checked");
            Claims.AddRange(
                from trElement in trElements
                select trElement.ParentElement.NextElementSibling.TextContent.Trim()
                );
        }

        private void InitProperties()
        {
            var elements = Document.QuerySelectorAll($"*[id*='{IdPrefix}']");
            foreach (var element in elements)
            {
                string kvpKey = element.Id[IdPrefix.Length..];
                string kvpValue = element switch
                {
                    IHtmlInputElement inputElement => inputElement.Value,
                    IHtmlTextAreaElement textArea => textArea.Value,
                    _ => throw new InvalidOperationException("Unsupported form element encountered."),
                };
                PropertyValues.Add(kvpKey, kvpValue);
            }
        }

        internal Edit SetClaims(IEnumerable<string> roleClaims)
        {
            Claims.Clear();
            Claims.AddRange(roleClaims);

            return this;
        }

        internal Edit UpdateProperties(IDictionary<string,string> newPropertyValues)
        {
            foreach(var kvp in newPropertyValues)
            {
                Assert.Contains(kvp.Key, PropertyValues.Keys);
                PropertyValues[kvp.Key] = kvp.Value;
            }

            return this;
        }

    }
}
