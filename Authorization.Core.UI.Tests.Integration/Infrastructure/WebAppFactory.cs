using Authorization.Core.UI.Test.Web;
using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Extensions;
using Authorization.Core.UI.Tests.Integration.Pages;
using CRFricke.Authorization.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    public class WebAppFactory : WebApplicationFactory<Startup>
    {
        internal const string AppRoleId = "576a2fa1-c883-47fe-8a8c-2b68f3e57143";
        internal const string AppUserId = "c59009a9-5ea5-4ee3-81f2-4cb5494f00f7";
        internal const string AppUserEmail = "AppUser@company.com";
        internal const string AppUserPassword = "AppUserP@ssw0rd!";

        private readonly SqliteConnection _connection = new SqliteConnection($"DataSource=:memory:");

        public WebAppFactory()
        {
            _connection.Open();

            ClientOptions.AllowAutoRedirect = false;
            ClientOptions.BaseAddress = new Uri("https://localhost");
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(whb => { });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseStartup<Startup>();

            builder.ConfigureServices(sc =>
            {
                sc.SetupTestDatabase<ApplicationDbContext>(_connection)
                    .AddMvc()
                    // Mark the cookie as essential for right now, as Identity uses it on
                    // several places to pass important data in post-redirect-get flows.
                    .AddCookieTempDataProvider(o => o.Cookie.IsEssential = true);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var result = base.CreateHost(builder);
            EnsureDatabaseCreated(result.Services);
            return result;
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var result = base.CreateServer(builder);
            EnsureDatabaseCreated(result.Host.Services);
            return result;
        }

        public void EnsureDatabaseCreated(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
            SeedDatabase(dbContext);
        }

        private void SeedDatabase(ApplicationDbContext dbContext)
        {
            var role = dbContext.Roles
                .Where(ar => ar.Id == AppRoleId)
                .AsNoTracking()
                .FirstOrDefault();
            if (role == null)
            {
                role = new ApplicationRole { Id = AppRoleId, Name = "AppRole" };
                dbContext.Roles.Add(role);
                dbContext.SaveChanges();
            }

            var user = dbContext.Users
                .Where(au => au.Id == AppUserId)
                .AsNoTracking()
                .FirstOrDefault();
            if (user == null)
            {
                user = InitializeAppUser(role);
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }
        }

        private static ApplicationUser InitializeAppUser(ApplicationRole role)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            var normalizer = new UpperInvariantLookupNormalizer();

            ApplicationUser user = new ApplicationUser
            {
                Id = AppUserId,
                Email = AppUserEmail,
                EmailConfirmed = true,
                GivenName = "Application",
                Surname = "User"
            };
            user.PasswordHash = hasher.HashPassword(user, AppUserPassword);
            user.NormalizedEmail = normalizer.NormalizeEmail(user.Email);
            user.UserName = user.Email;
            user.NormalizedUserName = normalizer.NormalizeName(user.UserName);
            user.SetClaims(new[] { role.Name });

            return user;
        }

        internal static async Task<Pages.Index> LoginExistingUserAsync(HttpClient client, string userName, string password)
        {
            var login = await Login.CreateAsync(client);
            return await login.LoginValidUserAsync(userName, password);
        }

        protected override void Dispose(bool disposing)
        {
            _connection.Dispose();

            base.Dispose(disposing);
        }
    }
}
