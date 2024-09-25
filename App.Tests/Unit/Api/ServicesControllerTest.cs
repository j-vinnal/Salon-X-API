using System.Security.Claims;
using App.BLL;
using App.Contracts.BLL;
using App.DAL.EF;
using App.Domain;
using App.Domain.Enums;
using App.Domain.Identity;
using AutoMapper;
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
public class ServicesControllerTest : IClassFixture<SharedContext>
{
    private readonly IAppBLL _bll;

    // sut - system under test
    private readonly ServicesController _controller;

    private readonly AppDbContext _ctx;

    private readonly UserManager<AppUser> _userManager;

    public ServicesControllerTest(SharedContext sharedContext)
    {
        _ctx = sharedContext.Ctx;
        // Initialize mappers
        // Mapper for UOW
        var configUow = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Service, DTO.DAL.Service>().ReverseMap();
            cfg.CreateMap<Company, DTO.DAL.Company>().ReverseMap();
        });

        var mapperUow = configUow.CreateMapper();

        // Mapper for BLL
        var configBll = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DTO.DAL.Service, DTO.BLL.Service>().ReverseMap();
            cfg.CreateMap<DTO.DAL.Company, DTO.BLL.Company>().ReverseMap();
        });

        var mapperBll = configBll.CreateMapper();

        // Mapper for Web
        var configWeb = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DTO.BLL.Service, DTO.Public.v1.Service>().ReverseMap();
            cfg.CreateMap<DTO.BLL.Company, DTO.Public.v1.Company>().ReverseMap();
        });

        var mapperWeb = configWeb.CreateMapper();

        // Initialize UOW and BLL
        var uow = new AppUOW(_ctx, mapperUow);

        _bll = new AppBLL(uow, mapperBll);

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

        // sut - system under test
        _controller = new ServicesController(_bll, mapperWeb);
    }

    private async Task<(AppUser, Company)> CreateUserAndCompanyAndSetClaims()
    {
        var userId = Guid.NewGuid();
        var testUser = new AppUser
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"testuser-{userId}@example.com"
        };
        await _userManager.CreateAsync(testUser, "Foo.Bar1");
        _ctx.Users.Add(testUser);
        await _ctx.SaveChangesAsync();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var company = new Company
        {
            Id = Guid.NewGuid(),
            CompanyName = $"TestCompany-{userId}",
            PublicUrl = $"test-company-{Guid.NewGuid()}",
            AppUserId = userId,
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        return (testUser, company);
    }

    [Fact]
    public async Task GetServicesTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var service = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = $"TestService-{testCompany.Id}",
            Description = "TestDescription",
            Price = 50.0M,
            Duration = 60,
            Status = EServiceStatus.Active,
            CompanyId = testCompany.Id
        };
        _ctx.Services.Add(service);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.GetServices();
        var okResult = result.Result as OkObjectResult;
        var data = okResult?.Value as IEnumerable<DTO.Public.v1.Service>;

        // Assert
        Assert.NotNull(okResult);
        Assert.NotNull(data);
        Assert.Single(data);
    }

    [Fact]
    public async Task CreateServiceTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var newService = new DTO.Public.v1.Service
        {
            Id = Guid.NewGuid(),
            ServiceName = $"NewService-{testCompany.Id}",
            Description = "NewDescription",
            Price = 70.0M,
            Duration = 45,
            Status = EServiceStatus.Active,
            CompanyId = testCompany.Id
        };

        // Act
        var result = await _controller.PostService(newService);
        var createdResult = result.Result as CreatedAtActionResult;
        var createdService = createdResult?.Value as DTO.Public.v1.Service;

        // Assert
        Assert.NotNull(createdResult);
        Assert.NotNull(createdService);
        Assert.Equal(newService.Id, createdService?.Id);
    }

    [Fact]
    public async Task UpdateServiceTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var service = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = $"TestService-{testCompany.Id}",
            Description = "TestDescription",
            Price = 50.0M,
            Duration = 60,
            Status = EServiceStatus.Active,
            CompanyId = testCompany.Id
        };
        _ctx.Services.Add(service);
        await _ctx.SaveChangesAsync();

        var updatedService = new DTO.Public.v1.Service
        {
            Id = service.Id,
            ServiceName = $"UpdatedService-{testCompany.Id}",
            Description = "UpdatedDescription",
            Price = 80.0M,
            Duration = 90,
            Status = EServiceStatus.Inactive,
            CompanyId = service.CompanyId
        };

        // Detach the existing entity to avoid tracking conflicts
        _ctx.Entry(service).State = EntityState.Detached;

        // Act
        var result = await _controller.PutService(updatedService.Id, updatedService);
        var updatedResult = result.Result as CreatedAtActionResult;
        var updatedData = updatedResult?.Value as DTO.Public.v1.Service;

        // Assert
        Assert.NotNull(updatedResult);
        Assert.NotNull(updatedData);
        Assert.Equal(updatedService.ServiceName, updatedData?.ServiceName);
        Assert.Equal(updatedService.Description, updatedData?.Description);
        Assert.Equal(updatedService.Price, updatedData?.Price);
        Assert.Equal(updatedService.Duration, updatedData?.Duration);
        Assert.Equal(updatedService.Status, updatedData?.Status);
    }

    [Fact]
    public async Task DeleteServiceTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var service = new Service
        {
            Id = Guid.NewGuid(),
            ServiceName = $"TestService-{testCompany.Id}",
            Description = "TestDescription",
            Price = 50.0M,
            Duration = 60,
            Status = EServiceStatus.Active,
            CompanyId = testCompany.Id
        };
        _ctx.Services.Add(service);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteService(service.Id);
        var noContentResult = result as NoContentResult;

        // Clear the change tracker to ensure the context is not tracking the deleted entity
        _ctx.ChangeTracker.Clear();

        var deletedService = await _ctx.Services.FindAsync(service.Id);

        // Assert
        Assert.NotNull(noContentResult);
        Assert.Null(deletedService);
    }
}