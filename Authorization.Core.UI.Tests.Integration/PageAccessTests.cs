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
        public async void PageAccessTest01(string endpoint, bool needsId)
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
        public async void PageAccessTest02()
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(Test.Web.AppGuids.Role.CalendarManager)
                );

            var response = await client.GetAsync("/Admin/Calendar");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "Can access Management pages in default area ")]
        [MemberData(nameof(Test03Data))]
        public async void PageAccessTest03Async(string endpoint, bool needsId)
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
        public async void PageAccessTest04Async(string endpoint)
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
        public async void PageAccessTest05Async(string endpoint, string claim, bool needsId)
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
        public async void PageAccessTest06Async(string endpoint)
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(SysGuids.Role.Administrator)
                );

            var response = await client.GetAsync(endpoint);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory(DisplayName = "Returns NotFound for non-existing Id ")]
        [MemberData(nameof(Test06Data))]
        public async void PageAccessTest07Async(string endpoint)
        {
            var client = CreateClientWithAuthenticationScheme(
                nameof(SysGuids.Role.Administrator)
                );

            var response = await client.GetAsync($"{endpoint}?Id={Guid.Empty}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        public static IEnumerable<object[]> Test01Data => new List<object[]>
        {
            new object[] { "/Admin/Role", false },
            new object[] { "/Admin/Role/Create", false },
            new object[] { "/Admin/Role/Delete", true },
            new object[] { "/Admin/Role/Details", true },
            new object[] { "/Admin/Role/Edit", true },
            new object[] { "/Admin/Role/Index", false },
            new object[] { "/Admin/User", false },
            new object[] { "/Admin/User/Create", false },
            new object[] { "/Admin/User/Delete", true },
            new object[] { "/Admin/User/Details", true },
            new object[] { "/Admin/User/Edit", true },
            new object[] { "/Admin/User/Index", false }
        };

        public static IEnumerable<object[]> Test03Data => new List<object[]>
        {
            new object[] { "/Authorization/Role", false },
            new object[] { "/Authorization/Role/Create", false },
            new object[] { "/Authorization/Role/Delete", true },
            new object[] { "/Authorization/Role/Details", true },
            new object[] { "/Authorization/Role/Edit", true },
            new object[] { "/Authorization/Role/Index", false },
            new object[] { "/Authorization/User", false },
            new object[] { "/Authorization/User/Create", false },
            new object[] { "/Authorization/User/Delete", true },
            new object[] { "/Authorization/User/Details", true },
            new object[] { "/Authorization/User/Edit", true },
            new object[] { "/Authorization/User/Index", false }
        };

        public static IEnumerable<object[]> Test04Data => new List<object[]>
        {
            new object[] { "/Admin/Role" },
            new object[] { "/Admin/Role/Create" },
            new object[] { "/Admin/Role/Delete" },
            new object[] { "/Admin/Role/Details" },
            new object[] { "/Admin/Role/Edit" },
            new object[] { "/Admin/Role/Index" },
            new object[] { "/Admin/User" },
            new object[] { "/Admin/User/Create" },
            new object[] { "/Admin/User/Delete" },
            new object[] { "/Admin/User/Details" },
            new object[] { "/Admin/User/Edit" },
            new object[] { "/Admin/User/Index" }
        };

        public static IEnumerable<object[]> Test05Data => new List<object[]>
        {
            new object[] { "/Admin/Role", SysClaims.Role.List, false },
            new object[] { "/Admin/Role/Create", SysClaims.Role.Create, false },
            new object[] { "/Admin/Role/Delete", SysClaims.Role.Delete, true },
            new object[] { "/Admin/Role/Details", SysClaims.Role.Read, true },
            new object[] { "/Admin/Role/Edit", SysClaims.Role.Update, true },
            new object[] { "/Admin/Role/Index", SysClaims.Role.List, false },
            new object[] { "/Admin/User", SysClaims.User.List, false },
            new object[] { "/Admin/User/Create", SysClaims.User.Create, false },
            new object[] { "/Admin/User/Delete", SysClaims.User.Delete, true },
            new object[] { "/Admin/User/Details", SysClaims.User.Read, true },
            new object[] { "/Admin/User/Edit", SysClaims.User.Update, true },
            new object[] { "/Admin/User/Index", SysClaims.User.List, false }
        };

        public static IEnumerable<object[]> Test06Data => new List<object[]>
        {
            new object[] { "/Admin/Role/Delete" },
            new object[] { "/Admin/Role/Details" },
            new object[] { "/Admin/Role/Edit" },
            new object[] { "/Admin/User/Delete" },
            new object[] { "/Admin/User/Details" },
            new object[] { "/Admin/User/Edit" }
        };
    }
}
