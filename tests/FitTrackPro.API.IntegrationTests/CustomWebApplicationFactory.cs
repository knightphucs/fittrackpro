namespace FitTrackPro.API.IntegrationTests;

using System.Data.Common;
using FitTrackPro.API.IntegrationTests.Authentication;
using FitTrackPro.API.IntegrationTests.Services.Authentication;
using FitTrackPro.API.IntegrationTests.Services.Caching;
using FitTrackPro.API.IntegrationTests.Services.Email;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Domain.Constants;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Infrastructure.Persistence;
using FitTrackPro.Infrastructure.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real PostgreSQL DbContext
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll<IApplicationDbContext>();

            // Remove Infrastructure services
            services.RemoveAll(typeof(IEmailService));
            services.RemoveAll(typeof(IJwtTokenGenerator));
            services.RemoveAll(typeof(IPasswordHasher));

            // Create SQLite In-Memory DB
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

            // Register Identity for tests (UserManager, SignInManager, etc.)
            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false; // tests should not require email confirmation
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Register the Fake Cache Service
            services.AddSingleton<ICacheService, FakeCacheService>();

            // Add test versions of missing services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, FakeJwtTokenGenerator>();
            services.AddScoped<IEmailService, FakeEmailService>();

            // Replace authentication with Fake TestAuth
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            services.PostConfigure<AuthenticationOptions>(opt =>
            {
                opt.DefaultAuthenticateScheme = "Test";
                opt.DefaultChallengeScheme = "Test";
            });

            // Remove Hangfire background job server — it tries to connect to PostgreSQL
            // (unavailable in tests) and causes TaskCanceledException on teardown.
            services.RemoveAll(typeof(IHostedService));

            // Create DB schema
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            if (!roleManager.RoleExistsAsync(Roles.User).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole<Guid>(Roles.User)).GetAwaiter().GetResult();
            }

            if (!roleManager.RoleExistsAsync(Roles.Administrator).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole<Guid>(Roles.Administrator)).GetAwaiter().GetResult();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}
