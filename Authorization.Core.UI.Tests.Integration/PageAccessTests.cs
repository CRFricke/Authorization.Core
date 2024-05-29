using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Infrastructure;
using CRFricke.Authorization.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Authorization.Core.UI.Tests.Integration
{
    #region Test AuthenticationHandler

    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Options.ClaimsIssuer ?? "Administrator";
            var claims = new[] 
            {
                new Claim(ClaimTypes.NameIdentifier, "8156bb9b-f56e-4f83-8a11-b0418b843e9b"),
                new Claim(ClaimTypes.Name, "Admin@company.com"),
                new Claim(ClaimTypes.Email, "Admin@company.com"),
                new Claim(ClaimTypes.Role, role),
            };
            var identity = new ClaimsIdentity(claims, "Identity.Application");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");

            var result = AuthenticateResult.Success(ticket);

            return Task.FromResult(result);
        }
    }

    #endregion

    public class PageAccessTests : IClassFixture<WebAppFactory>
    {
        public PageAccessTests(WebAppFactory factory)
        {
            Factory = factory;
        }

        public WebAppFactory Factory { get; }

        private string HostUri { get; } = "https://localhost/";


        private HttpClient CreateClientWithAuthenticationScheme(string claimsIssuer = "Administrator", bool allowRedirect = true)
        {
            var client = Factory.WithWebHostBuilder(builder => {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication(defaultScheme: "TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "TestScheme", options => { options.ClaimsIssuer = claimsIssuer; });
                });
            }).CreateClient(new()
            {
                AllowAutoRedirect = allowRedirect, BaseAddress = new(HostUri)
            });

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(scheme: "TestScheme");

            return client;
        }


        [Theory(DisplayName = "Can access Management page via friendly area name ")]
        [MemberData(nameof(Test01Data))]
        public async Task PageAccessTest01(string endpoint, bool needsId)
        {
            var client = CreateClientWithAuthenticationScheme();

            if (needsId)
            {
                var id = (endpoint.Contains("Role"))
                    ? Test.Web.AppGuids.Role.CalendarManager
                    : Test.Web.AppGuids.User.CalendarGuy;
                endpoint += $"?id={id}";
            }

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "Can access customer page actually located in friendly name area")]
        public async Task PageAccessTest02()
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(Test.Web.AppGuids.Role.CalendarManager)
                );

            var response = await client.GetAsync("/Admin/Calendar");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "Can access Management pages in default area ")]
        [MemberData(nameof(Test03Data))]
        public async Task PageAccessTest03Async(string endpoint, bool needsId)
        {
            var client = CreateClientWithAuthenticationScheme();

            if (needsId)
            {
                var id = (endpoint.Contains("Role"))
                    ? Test.Web.AppGuids.Role.CalendarManager
                    : Test.Web.AppGuids.User.CalendarGuy;
                endpoint += $"?id={id}";
            }

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "Redirects to AccessDenied page without proper claim ")]
        [MemberData(nameof(Test04Data))]
        public async Task PageAccessTest04Async(string endpoint)
        {
            var client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false, BaseAddress = new(HostUri)
            });

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Theory(DisplayName = "Can access management page with proper claim ")]
        [MemberData(nameof(Test05Data))]
        public async Task PageAccessTest05Async(string endpoint, string claim, bool needsId)
        {
            using var scope = Factory.Services.CreateScope();
            var authManager = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var role = dbContext.Roles.Find(Test.Web.AppGuids.Role.DocumentManager)!.SetClaims(claim);
            dbContext.Roles.Update(role);
            dbContext.SaveChanges();
            authManager.RefreshRole(role.Id);

            var client = CreateClientWithAuthenticationScheme(
                nameof(Test.Web.AppGuids.Role.DocumentManager)
                );

            if (needsId)
            {
                var id = (endpoint.Contains("Role"))
                    ? Test.Web.AppGuids.Role.CalendarManager
                    : Test.Web.AppGuids.User.CalendarGuy;
                endpoint += $"?id={id}";
            }

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "Returns NotFound for null Id ")]
        [MemberData(nameof(Test06Data))]
        public async Task PageAccessTest06Async(string endpoint)
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(SysGuids.Role.Administrator)
                );

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory(DisplayName = "Returns NotFound for non-existing Id ")]
        [MemberData(nameof(Test06Data))]
        public async Task PageAccessTest07Async(string endpoint)
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(SysGuids.Role.Administrator)
                );

            var response = await client.GetAsync($"{endpoint}?Id={Guid.Empty}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        public static TheoryData<string, bool> Test01Data => new()
        {
            { "/Admin/Role", false },
            { "/Admin/Role/Create", false },
            { "/Admin/Role/Delete", true },
            { "/Admin/Role/Details", true },
            { "/Admin/Role/Edit", true },
            { "/Admin/Role/Index", false },
            { "/Admin/User", false },
            { "/Admin/User/Create", false },
            { "/Admin/User/Delete", true },
            { "/Admin/User/Details", true },
            { "/Admin/User/Edit", true },
            { "/Admin/User/Index", false }
        };

        public static TheoryData<string, bool> Test03Data => new()
        {
            { "/Authorization/Role", false },
            { "/Authorization/Role/Create", false },
            { "/Authorization/Role/Delete", true },
            { "/Authorization/Role/Details", true },
            { "/Authorization/Role/Edit", true },
            { "/Authorization/Role/Index", false },
            { "/Authorization/User", false },
            { "/Authorization/User/Create", false },
            { "/Authorization/User/Delete", true },
            { "/Authorization/User/Details", true },
            { "/Authorization/User/Edit", true },
            { "/Authorization/User/Index", false }
        };

        public static TheoryData<string> Test04Data => new()
        {
            { "/Admin/Role" },
            { "/Admin/Role/Create" },
            { "/Admin/Role/Delete" },
            { "/Admin/Role/Details" },
            { "/Admin/Role/Edit" },
            { "/Admin/Role/Index" },
            { "/Admin/User" },
            { "/Admin/User/Create" },
            { "/Admin/User/Delete" },
            { "/Admin/User/Details" },
            { "/Admin/User/Edit" },
            { "/Admin/User/Index" }
        };

        public static TheoryData<string, string, bool> Test05Data => new()
        {
            { "/Admin/Role", SysClaims.Role.List, false },
            { "/Admin/Role/Create", SysClaims.Role.Create, false },
            { "/Admin/Role/Delete", SysClaims.Role.Delete, true },
            { "/Admin/Role/Details", SysClaims.Role.Read, true },
            { "/Admin/Role/Edit", SysClaims.Role.Update, true },
            { "/Admin/Role/Index", SysClaims.Role.List, false },
            { "/Admin/User", SysClaims.User.List, false },
            { "/Admin/User/Create", SysClaims.User.Create, false },
            { "/Admin/User/Delete", SysClaims.User.Delete, true },
            { "/Admin/User/Details", SysClaims.User.Read, true },
            { "/Admin/User/Edit", SysClaims.User.Update, true },
            { "/Admin/User/Index", SysClaims.User.List, false }
        };

        public static TheoryData<string> Test06Data => new()
        {
            { "/Admin/Role/Delete" },
            { "/Admin/Role/Details" },
            { "/Admin/Role/Edit" },
            { "/Admin/User/Delete" },
            { "/Admin/User/Details" },
            { "/Admin/User/Edit" }
        };
    }
}
