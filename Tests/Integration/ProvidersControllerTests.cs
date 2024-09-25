using System.Net;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Tests.Helpers;
using Xunit.Abstractions;

namespace Tests.Integration;

public class ProvidersControllerTests : IClassFixture<CustomWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public ProvidersControllerTests(CustomWebAppFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }


    [Fact(DisplayName = "Get - check that providers page loads")]
    public async Task DefaultPageTest()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/Providers");

        // Assert
        response.EnsureSuccessStatusCode();
    }


    //TODO Not working after adding the [Authorize] attribute to the controller
    // Unit test for adding a new provider
    [Fact(DisplayName = "Add new provider")]
    public async Task AddNewProviderTest()
    {
        // Load the form
        var response = await _client.GetAsync("Providers/Create/");

        response.EnsureSuccessStatusCode();

        // Get the actual content from the response
        var getTestContent = await HtmlHelpers.GetDocumentAsync(response);

        // Get the form and the request verification token
        var form = (IHtmlFormElement)getTestContent.QuerySelector("form[action='/Providers/Create'][method='post']")!;

        var requestVerificationToken =
            (IHtmlInputElement)getTestContent.QuerySelector("input[name=__RequestVerificationToken]")!;

        //Check that this is correct form
        Assert.Equal("/Providers/Create", form.Action);


        var companyName = Guid.NewGuid().ToString();

        // Prepare form values
        var formValues = PrepareFormValues(companyName, requestVerificationToken.Value);

        var postRegisterResponse = await _client.SendAsync(form, formValues);
        Assert.Equal(HttpStatusCode.Found, postRegisterResponse.StatusCode);

        var indexResponse = await _client.GetAsync(postRegisterResponse.Headers.Location);
        indexResponse.EnsureSuccessStatusCode();

        var indexContent = await indexResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains(companyName, indexContent);
    }

    private static Dictionary<string, string> PrepareFormValues(string companyName, string requestVerificationToken)
    {
        return new Dictionary<string, string>
        {
            ["CompanyName"] = companyName,
            ["RegistrationCode"] = "123456789",
            ["AppUserId"] = "0b439aaf-10f3-4c7d-b884-740097bbdd7a",
            ["__RequestVerificationToken"] = requestVerificationToken
        };
    }
}