using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using App.DAL.EF;
using App.Domain.Identity;

public class SharedContext : IDisposable
{
    public readonly IServiceScope Scope;
    public readonly AppDbContext Ctx;
    private readonly IServiceProvider ServiceProvider;
    private readonly IConfiguration Configuration;
    
    public SharedContext()
    {
        // Initialize configuration from file
        Configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Initialize services
        var services = new ServiceCollection();

        // Register IConfiguration instance
        services.AddSingleton<IConfiguration>(Configuration);

        var useInMemoryDatabase = Configuration.GetValue<bool>("Testing:UseInMemoryDatabase");
        if (useInMemoryDatabase)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));
        }
        else
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("TestConnection")));
        }

        services.AddIdentityCore<AppUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddLogging();

        ServiceProvider = services.BuildServiceProvider();

        Scope = ServiceProvider.CreateScope();
        Ctx = Scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure database is updated with the latest migrations
        if (!useInMemoryDatabase)
        {
            Ctx.Database.Migrate();
        }
    }

    public void Dispose()
    {
        // Dispose is intentionally left blank.
        // We'll handle deletion in the fixture's specialize method.
    }

    public void EnsureDeleted()
    {
        var deleteDatabaseAfterTests = Configuration.GetValue<bool>("Testing:DeleteDatabaseAfterTests");
        if (!Configuration.GetValue<bool>("Testing:UseInMemoryDatabase") && deleteDatabaseAfterTests)
        {
            Ctx.Database.EnsureDeleted();
        }
    }
}