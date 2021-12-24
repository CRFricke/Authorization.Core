using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization.Core.UI.Tests.Integration.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(
            this HttpClient client,
            IHtmlFormElement form,
            IHtmlElement submitButton,
            IEnumerable<KeyValuePair<string, string>> formValues)
        {
            foreach (var kvp in formValues)
            {
                switch (form[kvp.Key])
                {
                    case IHtmlInputElement inputElement:
                        inputElement.Value = kvp.Value;
                        break;
                    case IHtmlTextAreaElement textArea:
                        textArea.Value = kvp.Value;
                        break;
                    default:
                        throw new Exception("Unsupported form element encountered.");
                }
            }

            var submit = form.GetSubmission(submitButton);
            var target = (Uri)submit.Target;
            if (submitButton.HasAttribute("formaction"))
            {
                var formaction = submitButton.GetAttribute("formaction");
                target = new Uri(formaction, UriKind.Relative);
            }
            var submision = new HttpRequestMessage(new HttpMethod(submit.Method.ToString()), target)
            {
                Content = new StreamContent(submit.Body)
            };

            foreach (var header in submit.Headers)
            {
                submision.Headers.TryAddWithoutValidation(header.Key, header.Value);
                submision.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return client.SendAsync(submision);
        }
    }
}
