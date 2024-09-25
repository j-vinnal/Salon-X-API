using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit.Abstractions;

namespace Tests.Integration.Api;

public class ApiTests : IClassFixture<CustomWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public ApiTests(CustomWebAppFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }


    [Fact(DisplayName = "Get - check providers api")]
    public async Task ApiProvidersTest()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync("/api/Providers");
        var content = await response.Content.ReadAsStringAsync();
        //  var providers = JsonConvert.DeserializeObject<List<Provider>>(content);

        // Assert
        response.EnsureSuccessStatusCode();
        //    _testOutputHelper.WriteLine("Providers: " + JsonConvert.SerializeObject(providers, Formatting.Indented));
        //Assert.NotEmpty(providers);
    }
}