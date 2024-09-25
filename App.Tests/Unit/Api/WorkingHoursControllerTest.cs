using System.Security.Claims;
using App.BLL;
using App.Contracts.BLL;
using App.Contracts.DAL;
using App.DAL.EF;
using App.Domain;
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
public class WorkingHoursControllerTest : IClassFixture<SharedContext>
{
    private readonly IAppBLL _bll;

    // sut - system under test
    private readonly WorkingHoursController _controller;
    private readonly AppDbContext _ctx;
    private readonly IAppUOW _uow;

    private readonly UserManager<AppUser> _userManager;

    public WorkingHoursControllerTest(SharedContext sharedContext)
    {
        _ctx = sharedContext.Ctx;
        // Initialize mappers
        var configUow = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<WorkingHour, DTO.DAL.WorkingHour>().ReverseMap();
            cfg.CreateMap<Company, DTO.DAL.Company>().ReverseMap();
        });
        var mapperUow = configUow.CreateMapper();

        var configBll = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DTO.DAL.WorkingHour, DTO.BLL.WorkingHour>().ReverseMap();
            cfg.CreateMap<DTO.DAL.Company, DTO.BLL.Company>().ReverseMap();
        });
        var mapperBll = configBll.CreateMapper();

        var configWeb = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DTO.BLL.WorkingHour, DTO.Public.v1.WorkingHour>().ReverseMap();
            cfg.CreateMap<DTO.BLL.Company, DTO.Public.v1.Company>().ReverseMap();
        });
        var mapperWeb = configWeb.CreateMapper();


        _uow = new AppUOW(_ctx, mapperUow);
        _bll = new AppBLL(_uow, mapperBll);

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

        // sut
        _controller = new WorkingHoursController(_bll, mapperWeb);
    }

    private async Task<(AppUser, Company)> CreateUserAndCompanyAndSetClaims()
    {
        var userId = Guid.NewGuid();
        var testUser = new AppUser
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "testuser@example.com"
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
            CompanyName = "TestCompany",
            PublicUrl = "test-company-" + Guid.NewGuid(),
            AppUserId = userId,
            User = testUser
        };
        _ctx.Companies.Add(company);
        await _ctx.SaveChangesAsync();

        return (testUser, company);
    }

    [Fact]
    public async Task GetWorkingHoursTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var workingHour = new WorkingHour
        {
            Id = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsActive = true,
            CompanyId = testCompany.Id
        };
        _ctx.WorkingHours.Add(workingHour);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorkingHours();
        var okResult = result.Result as OkObjectResult;
        var data = okResult?.Value as IEnumerable<DTO.Public.v1.WorkingHour>;

        // Assert
        Assert.NotNull(okResult);
        Assert.NotNull(data);
        Assert.Single(data);
    }

    [Fact]
    public async Task CreateWorkingHourTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var newWorkingHour = new DTO.Public.v1.WorkingHour
        {
            Id = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsActive = true,
            CompanyId = testCompany.Id
        };

        // Act
        var result = await _controller.PostWorkingHour(newWorkingHour);
        var createdResult = result.Result as CreatedAtActionResult;
        var createdWorkingHour = createdResult?.Value as DTO.Public.v1.WorkingHour;

        // Assert
        Assert.NotNull(createdResult);
        Assert.NotNull(createdWorkingHour);
        Assert.Equal(newWorkingHour.Id, createdWorkingHour?.Id);
    }

    [Fact]
    public async Task UpdateWorkingHourTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var workingHour = new WorkingHour
        {
            Id = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsActive = true,
            CompanyId = testCompany.Id
        };
        _ctx.WorkingHours.Add(workingHour);
        await _ctx.SaveChangesAsync();

        var updatedWorkingHour = new DTO.Public.v1.WorkingHour
        {
            Id = workingHour.Id,
            DayOfWeek = DayOfWeek.Thursday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            IsActive = false,
            CompanyId = workingHour.CompanyId
        };

        _ctx.Entry(workingHour).State = EntityState.Detached;

        // Act
        var result = await _controller.PutWorkingHour(updatedWorkingHour.Id, updatedWorkingHour);
        var updatedResult = result.Result as CreatedAtActionResult;
        var updatedData = updatedResult?.Value as DTO.Public.v1.WorkingHour;

        // Assert
        Assert.NotNull(updatedResult);
        Assert.NotNull(updatedData);
        Assert.Equal(updatedWorkingHour.DayOfWeek, updatedData?.DayOfWeek);
        Assert.Equal(updatedWorkingHour.StartTime, updatedData?.StartTime);
        Assert.Equal(updatedWorkingHour.EndTime, updatedData?.EndTime);
        Assert.Equal(updatedWorkingHour.IsActive, updatedData?.IsActive);
    }

    [Fact]
    public async Task DeleteWorkingHourTest()
    {
        // Arrange
        var (testUser, testCompany) = await CreateUserAndCompanyAndSetClaims();

        var workingHour = new WorkingHour
        {
            Id = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Friday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            IsActive = false,
            CompanyId = testCompany.Id
        };
        _ctx.WorkingHours.Add(workingHour);
        await _ctx.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteWorkingHour(workingHour.Id);
        var noContentResult = result as NoContentResult;

        // Clear the change tracker to ensure the context is not tracking the deleted entity
        _ctx.ChangeTracker.Clear();

        var deletedWorkingHour = await _ctx.WorkingHours.FindAsync(workingHour.Id);

        // Assert
        Assert.NotNull(noContentResult);
        Assert.Null(deletedWorkingHour);
    }
}