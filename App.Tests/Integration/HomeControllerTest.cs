using App.DAL.EF;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace App.Tests.Integration;

[Collection("Database collection")]
public class HomeControllerTest : IClassFixture<SharedContext>, IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly AppDbContext _ctx;

    public HomeControllerTest(WebApplicationFactory<Program> factory, SharedContext sharedContext)
    {
        _ctx = sharedContext.Ctx; // Use the shared context

        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Index()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}