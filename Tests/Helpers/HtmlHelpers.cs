using System.Net.Http.Headers;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Helpers;

public static class HtmlHelpers
{
    public static async Task<IHtmlDocument> GetDocumentAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var document = await BrowsingContext.New()
            .OpenAsync(ResponseFactory, CancellationToken.None);
        return (IHtmlDocument)document;

        // make DOM from response
        void ResponseFactory(VirtualResponse htmlResponse)
        {
            htmlResponse
                .Address(response.RequestMessage?.RequestUri)
                .Status(response.StatusCode);

            MapHeaders(response.Headers);
            MapHeaders(response.Content.Headers);

            htmlResponse.Content(content);

            void MapHeaders(HttpHeaders headers)
            {
                foreach (var header in headers)
                foreach (var value in header.Value)
                    htmlResponse.Header(header.Key, value);
            }
        }
    }


    private static IHtmlFormElement SetFormValues(IHtmlFormElement form,
        IEnumerable<KeyValuePair<string, string>> formValues)
    {
        foreach (var (key, value) in formValues)
            switch (form[key])
            {
                case IHtmlInputElement:
                {
                    (form[key] as IHtmlInputElement)!.Value = value;
                    if ((form[key] as IHtmlInputElement)!.Type == "checkbox" && bool.Parse(value))
                        (form[key] as IHtmlInputElement)!.IsChecked = true;
                    break;
                }
                case IHtmlSelectElement:
                {
                    (form[key] as IHtmlSelectElement)!.Value = value;
                    break;
                }
            }

        return form;
    }

    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client, IHtmlFormElement form,
        IHtmlElement submitButton, IEnumerable<KeyValuePair<string, string>> formValues)
    {
        form = SetFormValues(form, formValues);

        var submit = form.GetSubmission(submitButton);
        var target = (Uri)submit!.Target;
        if (submitButton.HasAttribute("formaction"))
        {
            var formaction = submitButton.GetAttribute("formaction");
            target = new Uri(formaction!, UriKind.Relative);
        }

        var submission = new HttpRequestMessage(new HttpMethod(submit.Method.ToString()), target)
        {
            Content = new StreamContent(submit.Body)
        };

        foreach (var (key, value) in submit.Headers)
        {
            submission.Headers.TryAddWithoutValidation(key, value);
            submission.Content.Headers.TryAddWithoutValidation(key, value);
        }

        return client.SendAsync(submission);
    }


    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        IHtmlFormElement form,
        IHtmlElement submitButton)
    {
        return client.SendAsync(form, submitButton, new Dictionary<string, string>());
    }

    public static Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        IHtmlFormElement form,
        IEnumerable<KeyValuePair<string, string>> formValues)
    {
        var submitElement = Assert.Single(form.QuerySelectorAll("[type=submit]"));
        var submitButton = Assert.IsAssignableFrom<IHtmlElement>(submitElement);

        return client.SendAsync(form, submitButton, formValues);
    }
}