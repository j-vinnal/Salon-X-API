using System.Net.Http.Json;
using System.Text.Json;
using App.DTO.Public.v1.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Tests.Integration.Api.Identity;

public class IdentityIntegrationTests : IClassFixture<CustomWebAppFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebAppFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public IdentityIntegrationTests(CustomWebAppFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _factory = factory;
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }


    [Fact(DisplayName = "POST - register new user")]
    public async Task RegisterNewUser()
    {
        // Arrange
        var URL = "api/v1/identity/account/register";

        var registerData = new
        {
            Email = "testE@test.ee",
            Password = "Foo.bar1",
            Firstname = "Test Firstname",
            Lastname = "Test Lastname"
        };

        var data = JsonContent.Create(registerData);

        // Act
        var response = await _client.PostAsync(URL, data);

        // Assert
        Assert.True(response.IsSuccessStatusCode);


        var responseContent = await response.Content.ReadAsStringAsync();
        _testOutputHelper.WriteLine("------ Response Content -----");
        _testOutputHelper.WriteLine(responseContent);
        var jwtResponse = JsonSerializer.Deserialize<JWTResponse>(responseContent);

        Assert.NotNull(jwtResponse);
    }

    /*
    [Fact(DisplayName = "POST - login user")]
    public async Task LoginUser()
    {
        // Arrange

        // Act

        // Assert

    }



    //parameters, password too short, email missing etc
    [Fact(DisplayName = "POST - login user failed")]
    public async Task LoginUserFailed()
    {
        // Arrange

        // Act

        // Assert

    }

    [Fact(DisplayName = "POST - JWT expired")]
    public async Task JWTExpired()
    {
        // Arrange

        // Act

        // Assert

    }


    [Fact(DisplayName = "POST - JWT renewal")]
    public async Task JWTRenewal()
    {
        // Arrange

        // Act

        // Assert

    }

    [Fact(DisplayName = "POST - JWT Logout")]
    public async Task JWTLogout()
    {
        // Arrange

        // Act

        // Assert

    }
    */
}