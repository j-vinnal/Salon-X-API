using System.Security.Claims;
using App.BLL;
using App.BLL.Services;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.DAL.EF;
using App.Domain;
using App.Domain.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using WebApp.ApiControllers.Admin;

namespace App.Tests.Unit.Api;

[Collection("Database collection")]
public class CompaniesControllerTest : IClassFixture<SharedContext>
{
    // sut - system under test
    private readonly CompaniesController _controller;
    private readonly AppDbContext _ctx;

    private readonly UserManager<AppUser> _userManager;

    public CompaniesControllerTest(SharedContext sharedContext)
    {
        _ctx = sharedContext.Ctx;

        // Initialize mappers
        var configUow = new MapperConfiguration(cfg => cfg.CreateMap<Company, DTO.DAL.Company>().ReverseMap());
        var mapperUow = configUow.CreateMapper();

        var configBll = new MapperConfiguration(cfg => cfg.CreateMap<DTO.DAL.Company, DTO.BLL.Company>().ReverseMap());
        var mapperBll = configBll.CreateMapper();

        var configWeb =
            new MapperConfiguration(cfg => cfg.CreateMap<DTO.BLL.Company, DTO.Public.v1.Company>().ReverseMap());
        var mapperWeb = configWeb.CreateMapper();


        IAppUOW uow = new AppUOW(_ctx, mapperUow);
        IAppBLL bll = new AppBLL(uow, mapperBll);

        // UserManager
        var storeStub = Substitute.For<IUserStore<AppUser>>();
        var optionsStub = Substitute.For<IOptions<IdentityOptions>>();
        var hasherStub = Substitute.For<IPasswordHasher<AppUser>>();

        var validatorStub = Substitute.For<IEnumerable<IUserValidator<AppUser>>>();
        var passwordStub = Substitute.For<IEnumerable<IPasswordValidator<AppUser>>>();
        var lookupStub = Substitute.For<ILookupNormalizer>();
        var errorStub = Substitute.For<IdentityErrorDescriber>();
        var serviceStub = Substitute.For<IServiceProvider>();
        var loggerStub = Substitute.For<ILogger<UserManager<AppUser>>>();

        _userManager = Substitute.For<UserManager<AppUser>>(
            storeStub,
            optionsStub,
            hasherStub,
            validatorStub, passwordStub, lookupStub, errorStub, serviceStub, loggerStub
        );

        // Stub of IWebHostEnvironment
        var webHostEnvironmentStub = Substitute.For<IWebHostEnvironment>();
        webHostEnvironmentStub.WebRootPath.Returns("C:/Projects/Uni/2024/icd0024-23-24-s/BeautyHub/WebApp/wwwroot");

        // image upload service
        IImageUploadService imageUploadService = new ImageUploadService(webHostEnvironmentStub);

        // sut
        _controller = new CompaniesController(bll, mapperWeb, imageUploadService);
    }

    private async Task<AppUser> CreateUserAndSetClaims()
    {
        var userId = Guid.NewGuid();
        var testUser = new AppUser
        {
            Id = userId,
            FirstName = "Test", LastName = "User", Email = $"testuser-{userId}@example.com"
        };
        await _userManager.CreateAsync(testUser, "Foo.Bar1");
        _ctx.Users.Add(testUser);
        await _ctx.SaveChangesAsync();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return testUser;
    }

    [Fact]
    public async Task GetCompaniesTest()
    {
        // Arrange
        var testUser = await CreateUserAndSetClaims();

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"TestSalon-{testUser.Id}",
            PublicUrl = $"test-salon-{Guid.NewGuid()}",
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.GetCompanies();
        var okResult = result.Result as OkObjectResult;
        var data = okResult?.Value as IEnumerable<DTO.Public.v1.Company>;

        // Assert
        Assert.NotNull(okResult);
        Assert.NotNull(data);
        Assert.Single(data);
    }

    [Fact]
    public async Task CreateCompanyTest()
    {
        // Arrange
        await CreateUserAndSetClaims();

        var newCompany = new DTO.Public.v1.Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"NewSalon-{Guid.NewGuid()}",
            PublicUrl = $"new-salon-{Guid.NewGuid()}"
        };

        // Act
        var result = await _controller.PostCompany(newCompany);
        var createdResult = result.Result as CreatedAtActionResult;
        var createdCompany = createdResult?.Value as DTO.Public.v1.Company;

        // Assert
        Assert.NotNull(createdResult);
        Assert.NotNull(createdCompany);
        Assert.Equal(newCompany.Id, createdCompany?.Id);
    }

    [Fact]
    public async Task UpdateCompanyTest()
    {
        // Arrange
        var testUser = await CreateUserAndSetClaims();

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"DeleteSalon-{testUser.Id}",
            PublicUrl = $"delete-salon-{Guid.NewGuid()}",
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        var updatedCompany = new DTO.Public.v1.Company
        {
            Id = company.Id,
            CompanyName = $"UpdatedSalon-{testUser.Id}",
            PublicUrl = $"updated-salon-{Guid.NewGuid()}"
        };

        // Detach the existing entity to avoid tracking conflicts
        _ctx.Entry(company).State = EntityState.Detached;

        // Act
        var result = await _controller.PutCompany(updatedCompany.Id, updatedCompany);
        var updatedResult = result.Result as CreatedAtActionResult;
        var updatedData = updatedResult?.Value as DTO.Public.v1.Company;

        // Assert
        Assert.NotNull(updatedResult);
        Assert.NotNull(updatedData);
        Assert.Equal(updatedCompany.CompanyName, updatedData?.CompanyName);
    }

    [Fact]
    public async Task DeleteCompanyTest()
    {
        // Arrange
        var testUser = await CreateUserAndSetClaims();

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"DeleteSalon-{testUser.Id}",
            PublicUrl = $"delete-salon-{Guid.NewGuid()}",
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteCompany(company.Id);
        var noContentResult = result as NoContentResult;

        // Clear the change tracker to ensure the context is not tracking the deleted entity
        _ctx.ChangeTracker.Clear();

        var deletedCompany = await _ctx.Companies.FindAsync(company.Id);

        // Assert
        Assert.NotNull(noContentResult);
        Assert.Null(deletedCompany);
    }


    /*
    [Fact]
    public async Task GetCompanyByPublicUrlTest()
    {
        // Arrange
        var testUser = await CreateUserAndSetClaims();
        var publicUrl = $"test-salon-{Guid.NewGuid()}";

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"TestSalon-{testUser.Id}",
            PublicUrl = publicUrl,
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.GetCompanyByPublicUrl(publicUrl);
        var okResult = result.Result as OkObjectResult;
        var data = okResult?.Value as DTO.Public.v1.Company;

        // Assert
        Assert.NotNull(okResult);
        Assert.NotNull(data);
        Assert.Equal(company.CompanyName, data.CompanyName);
    }

    */
}