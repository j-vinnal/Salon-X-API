using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using App.BLL;
using App.BLL.Services;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.DAL.EF;
using App.DAL.EF.Seeding;
using App.Domain.Identity;
using App.Public;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;

//using App.DAL.EF.Seeding;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//Identity
builder.Services
    .AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)

    //Add custom error messages
    //  .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>();


// clear default claims
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
//Add JWT support
builder.Services
    .AddAuthentication()
    //Sliding expiration means that when a user is active, the timeout is reset (extended) for the cookie.
    .AddCookie(options => { options.SlidingExpiration = true; })
    .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = builder.Configuration.GetValue<string>("JWT:Issuer"),
                ValidAudience = builder.Configuration.GetValue<string>("JWT:Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:Key") ??
                                           throw new InvalidOperationException())),
                ClockSkew = TimeSpan.Zero
            };
        }
    );


builder.Services.AddControllersWithViews();



builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
    });
});



//add automapper configuration
builder.Services.AddAutoMapper(
    typeof(AutomapperConfig)
);


//Versioning
var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;

    //in case of no version specified, use the default version
    options.DefaultApiVersion = new ApiVersion(1, 0);
});

apiVersioningBuilder.AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; // 'v' prefix for versioning
        options.SubstituteApiVersionInUrl = true;
    }
);

builder.Services.AddEndpointsApiExplorer();


//If someone asks for an IOptions<SwaggerGenOptions>, give them an instance of ConfigureSwaggerOptions

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen();

//DI
builder.Services.AddScoped<IAppUOW, AppUOW>();
builder.Services.AddScoped<IAppBLL, AppBLL>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();


// I18N
var supportedCultures = builder.Configuration.GetSection("SupportedCultures")
    .GetChildren()
    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
    .Select(x => new CultureInfo(x.Value!))
    .ToArray();

var defaultCulture = builder.Configuration["DefaultCulture"] ??
                     throw new InvalidOperationException("DefaultCulture setting is missing or empty.");


builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.DefaultRequestCulture = new RequestCulture(
        defaultCulture,
        defaultCulture
    );

    options.SetDefaultCulture(defaultCulture);

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});


// ===============================================================
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // Listen on port 80
});

var app = builder.Build();
// ===============================================================

// Check if the application is running in a test environment
var isTestEnvironment = builder.Configuration.GetValue<bool>("Testing:IsTestEnvironment");

if (!isTestEnvironment)
{
    // Set up all the database stuff and seed initial data
    SetupAppData(app, app.Configuration);
}


// web server stuff =>

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseRouting();

app.UseCors("CorsAllowAll");

app.UseAuthorization();


var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (localizationOptions?.Value == null)
    throw new InvalidOperationException("RequestLocalizationOptions service is not available.");

app.UseRequestLocalization(localizationOptions.Value);


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
});


app.MapControllerRoute(
    "test",
    "foo/{bar:int}",
    new
    {
        Controller = "Home",
        Action = "Test"
    });


/*
app.MapControllerRoute(
    name: "getCompanyByName",
    pattern: "live/{companyName:alpha}",
    defaults: new
    {
        Controller = "Companies",
        Action = "GetCompanyByName"
    });
*/

/*
app.MapControllerRoute(
    name: "test",
    pattern: "foo/{bar:alpha}",
    defaults: new
    {
        Controller = "Home",
        Action = "TestAlpha"
    });
*/


//More specific routes should be defined first
//Area route
app.MapControllerRoute(
    "default",
    "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");



app.MapRazorPages();

app.Run();
return;


static async void SetupAppData(IApplicationBuilder app, IConfiguration configuration)
{
    //DI engine
    using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
        .CreateScope();
    
    var env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    AppDataInit.InitializeSeedDataPath(env);

    await using var context = serviceScope.ServiceProvider.GetService<AppDbContext>();


    if (context == null) throw new ApplicationException("Problem in services. Can't initialize DB Context");

    using var userManager = serviceScope.ServiceProvider.GetService<UserManager<AppUser>>();
    using var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<AppRole>>();

    if (userManager == null || roleManager == null)
        throw new ApplicationException("Problem in services. Can't initialize UserManager or RoleManager");

    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<IApplicationBuilder>>();


    if (logger == null) throw new ApplicationException("Problem in services. Can't initialize logger");

    if (context.Database.ProviderName!.Contains("InMemory")) return;


    //TODO: wait for db connection


    if (configuration.GetValue<bool>("DataInit:DropDatabase"))
    {
        logger.LogWarning("Dropping Database");
        AppDataInit.DropDatabase(context);
    }

    if (configuration.GetValue<bool>("DataInit:MigrateDatabase"))
    {
        logger.LogWarning("Migrating Database");
        AppDataInit.MigrateDatabase(context);
    }


    if (configuration.GetValue<bool>("DataInit:SeedIdentity"))
    {
        logger.LogWarning("Seeding identity");
        await AppDataInit.SeedIdentity(userManager, roleManager, logger);
    }

    if (configuration.GetValue<bool>("DataInit:SeedData"))
    {
        logger.LogWarning("Seeding app data...");
        var changedEntries = await AppDataInit.SeedAppData(context);
        logger.LogWarning($"Seeded app data: {changedEntries} entries changed");
    }
}


/// <summary>
///     The entry point for the application. Needed for unit testing, to change generated top level statement class to
///     public
/// </summary>
public partial class Program
{
}