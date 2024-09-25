//using App.DAL.EF.Seeding;

using App.DAL.EF;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Tests;

public class CustomWebAppFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            var configuration = sp.GetService<IConfiguration>();

            var useInMemoryDatabase = configuration!.GetValue<bool>("Testing:UseInMemoryDatabase");

            // find DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                     typeof(DbContextOptions<AppDbContext>));

            // if found - remove
            if (descriptor != null) services.Remove(descriptor);

            // and new DbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                if (useInMemoryDatabase)
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                }
                else
                {
                    var connectionString = configuration!.GetConnectionString("TestConnection");
                    options.UseNpgsql(connectionString);
                }
            });


            // create db and seed data
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            var logger = scopedServices
                .GetRequiredService<ILogger<CustomWebAppFactory<TStartup>>>();

            db.Database.EnsureCreated();

            /*
            // seed user
            try
            {
                Task.Run(() =>
                {
                    AppDataInit.SeedIdentity(
                        scopedServices.GetRequiredService<UserManager<AppUser>>(),
                        scopedServices.GetRequiredService<RoleManager<AppRole>>(),
                        db
                    ).Wait();
                }).Wait();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the " +
                                    "database with test users. Error: {Message}", ex.Message);
            }
            */
        });
    }
}