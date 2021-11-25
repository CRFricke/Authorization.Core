using Authorization.Core.Tests.Data;
using Fricke.Authorization.Core;
using Fricke.Authorization.Core.Attributes;
using Fricke.Test.Fakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

using AuthorizationFailure = Fricke.Authorization.Core.AuthorizationFailure;

namespace Authorization.Core.Tests
{
    delegate void TryGetValueDelegate(object key, out object value);

    public class AuthorizationManagerTests
    {
        [Fact(DisplayName = "AuthorizeAsync returns NoUserId")]
        public async Task AuthorizationManagerTest01Async()
        {
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Read);
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new Claim[] { } &&
                cp.Identity.Name == "TestUser@StEmilian.com"
                );

            var logger = new TestLogger<AuthorizationManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, appClaimRequirement);

            Assert.False(result.Succeeded);
            Assert.Equal(Fricke.Authorization.Core.AuthorizationFailure.Reason.NoUserId, result.Failure.FailureReason);
            Assert.Equal(appClaimRequirement.ClaimValues, result.Failure.FailingClaims);
        }

        [Fact(DisplayName = "AuthorizeAsync returns NotAuthorized")]
        public async Task AuthorizationManagerTest02Async()
        {
            var user = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole() { Name = "RoleManager" };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Create);

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(user.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { role.Id }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(role.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var logger = new TestLogger<AuthorizationManager>();

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, appClaimRequirement);

            Assert.False(result.Succeeded);
            Assert.Equal(AuthorizationFailure.Reason.NotAuthorized, result.Failure.FailureReason);
            Assert.Equal(appClaimRequirement.ClaimValues, result.Failure.FailingClaims);
        }

        [Fact(DisplayName = "AuthorizeAsync returns Succeeded for Administrator")]
        public async Task AuthorizationManagerTest03Async()
        {
            var user = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole() { Name = "Administrator" };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(user.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.Administrator }); }
                    )
                ).Returns(true);

            var logger = new TestLogger<AuthorizationManager>();

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, appClaimRequirement);

            Assert.True(result.Succeeded);
            Assert.Null(result.Failure);
        }

        [Fact(DisplayName = "AuthorizeAsync returns Succeeded")]
        public async Task AuthorizationManagerTest04Async()
        {
            var user = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole() { Name = "RoleManager" };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(user.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { role.Id }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(role.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var logger = new TestLogger<AuthorizationManager>();

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, appClaimRequirement);

            Assert.True(result.Succeeded);
            Assert.Null(result.Failure);
        }

        [Fact(DisplayName = "AuthorizeAsync loads UserRoleCache")]
        public async Task AuthorizationManagerTest05Async()
        {
            var user = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole() { Id = SysGuids.Role.Administrator, Name = nameof(SysGuids.Role.Administrator) };
            var userClaim = new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = role.Name };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var dbContext = Mock.Of<AppDbContext>(db =>
                db.Set<IdentityUserClaim<string>>() == new[] { userClaim }.AsQueryable().BuildMockDbSet().Object &&
                db.Set<AppRole>() == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var userRoleCache = new UserRoleCache();

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository<AppUser, AppRole>)) == dbContext &&
                sp.GetService(typeof(IHttpContextAccessor)) == new Mock<IHttpContextAccessor>().Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache
                );

            Mock.Get(serviceProvider.GetRequiredService<IHttpContextAccessor>())
                .Setup(hca => hca.HttpContext.RequestServices).Returns(serviceProvider);

            var logger = new TestLogger<AuthorizationManager>();

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, appClaimRequirement);

            Assert.True(result.Succeeded);

            var tryResult = userRoleCache.TryGetValue(user.Id, out HashSet<string> hashset);
            Assert.True(tryResult);
            Assert.Single(hashset);
            Assert.Contains(role.Id, hashset);

            userRoleCache.Dispose();
        }

        [Fact(DisplayName = "AuthorizeAsync returns ArgumentException")]
        public async Task AuthorizationManagerTest07Async()
        {
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Read);
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new Claim[] { } &&
                cp.Identity.Name == "TestUser@StEmilian.com"
                );

            var logger = new TestLogger<AuthorizationManager>();
            var serviceProvider = Mock.Of<IServiceProvider>();

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var ex = await Record.ExceptionAsync(async () =>
                await authorizationManager.AuthorizeAsync(claimsPrincipal, Guid.Empty.ToString(), appClaimRequirement)
                );

            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
            Assert.Contains($"does not implement {nameof(IRequiresAuthorization)} interface", ex.Message);
        }

        [Fact(DisplayName = "RefreshUser removes UserRoleCache entry")]
        public void AuthorizationManagerTest08()
        {
            object cacheKey = null;
            var user = new AppUser("TestUser@StEmilian.com");

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.Remove(user.Id))
                .Callback((object key) => { cacheKey = key; });

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            authorizationManager.RefreshUser(user.Id);

            Assert.Equal(user.Id, cacheKey);
        }

        [Fact(DisplayName = "RefreshRole removes RoleClaimCache entry")]
        public void AuthorizationManagerTest09()
        {
            object cacheKey = null;
            var role = new AppRole() { Name = "RoleManager" };

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.Remove(role.Id))
                .Callback((object key) => { cacheKey = key; });

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            authorizationManager.RefreshRole(role.Id);

            Assert.Equal(role.Id, cacheKey);
        }

        [Fact(DisplayName = "AuthorizeAsync [User] returns Succeeded for Administrator")]
        public async Task AuthorizationManagerTest10Async()
        {
            var user = new AppUser("TestUser@StEmilian.com");
            var newUser = new AppUser("NewUser@StEmilian.com");
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Create);

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(user.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.Administrator }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, newUser, appClaimRequirement);

            Assert.True(result.Succeeded);
        }

        [Fact(DisplayName = "AuthorizeAsync [User] returns Elevation for adding Administrator role")]
        public async Task AuthorizationManagerTest11Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole { Id = SysGuids.Role.Administrator, Name = nameof(SysGuids.Role.Administrator) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Create);
            var expectedClaims = new[] { nameof(SysGuids.Role.Administrator) };

            var user = new AppUser("NewUser@StEmilian.com");
            user.Claims.Add(
                new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = nameof(SysGuids.Role.Administrator) }
                );

            var dbContext = Mock.Of<AppDbContext>(db =>
                db.Set<AppRole>() == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.UserManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository<AppUser, AppRole>)) == dbContext &&
                sp.GetService(typeof(IHttpContextAccessor)) == new Mock<IHttpContextAccessor>().Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            Mock.Get(serviceProvider.GetRequiredService<IHttpContextAccessor>())
                .Setup(hca => hca.HttpContext.RequestServices).Returns(serviceProvider);

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, user, appClaimRequirement);

            Assert.False(result.Succeeded);
            Assert.Equal(AuthorizationFailure.Reason.Elevation, result.Failure.FailureReason);
            Assert.Equal(expectedClaims, result.Failure.FailingClaims);
        }

        [Fact(DisplayName = "AuthorizeAsync [User] returns Elevation")]
        public async Task AuthorizationManagerTest12Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole { Id = SysGuids.Role.RoleManager, Name = nameof(SysGuids.Role.RoleManager) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Create);
            var expectedClaims = SysClaims.Role.DefinedClaims;

            var user = new AppUser("NewUser@StEmilian.com");
            user.Claims.Add(
                new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = nameof(SysGuids.Role.RoleManager) }
                );
            user.Claims.Add(
                new IdentityUserClaim<string> { Id = 2, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = nameof(SysGuids.Role.UserManager) }
                );

            var dbContext = Mock.Of<AppDbContext>(db =>
                db.Set<AppRole>() == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.UserManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.RoleManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);


            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository<AppUser, AppRole>)) == dbContext &&
                sp.GetService(typeof(IHttpContextAccessor)) == new Mock<IHttpContextAccessor>().Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            Mock.Get(serviceProvider.GetRequiredService<IHttpContextAccessor>())
                .Setup(hca => hca.HttpContext.RequestServices).Returns(serviceProvider);

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, user, appClaimRequirement);

            Assert.False(result.Succeeded);
            Assert.Equal(AuthorizationFailure.Reason.Elevation, result.Failure.FailureReason);
            Assert.Equal(expectedClaims, result.Failure.FailingClaims);
        }

        [Fact(DisplayName = "AuthorizeAsync [User] returns Succeeded")]
        public async Task AuthorizationManagerTest13Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole { Id = SysGuids.Role.UserManager, Name = nameof(SysGuids.Role.UserManager) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Create);
            var expectedClaims = SysClaims.Role.DefinedClaims;

            var user = new AppUser("NewUser@StEmilian.com");
            user.Claims.Add(
                new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = nameof(SysGuids.Role.UserManager) }
                );

            var dbContext = Mock.Of<AppDbContext>(db =>
                db.Set<AppRole>() == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.UserManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository<AppUser, AppRole>)) == dbContext &&
                sp.GetService(typeof(IHttpContextAccessor)) == new Mock<IHttpContextAccessor>().Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            Mock.Get(serviceProvider.GetRequiredService<IHttpContextAccessor>())
                .Setup(hca => hca.HttpContext.RequestServices).Returns(serviceProvider);

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, user, appClaimRequirement);

            Assert.True(result.Succeeded);
            Assert.Null(result.Failure);
        }

        [Fact(DisplayName = "AuthorizeAsync [Role] returns Succeeded for Administrator")]
        public async Task AuthorizationManagerTest14Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var role = new AppRole { Name = "NewRole" };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.Administrator }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, role, appClaimRequirement);

            Assert.True(result.Succeeded);
        }

        [Fact(DisplayName = "AuthorizeAsync [Role] returns Elevation")]
        public async Task AuthorizationManagerTest15Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);
            var expectedClaims = new[] { SysClaims.User.Read };

            var role = new AppRole { Name = "NewRole" };
            role.Claims.Add(
                new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = SysClaims.Role.Read }
                );
            role.Claims.Add(
                new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = SysClaims.User.Read }
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.RoleManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.RoleManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, role, appClaimRequirement);

            Assert.False(result.Succeeded);
            Assert.Equal(AuthorizationFailure.Reason.Elevation, result.Failure.FailureReason);
            Assert.Equal(expectedClaims, result.Failure.FailingClaims);
        }

        [Fact(DisplayName = "AuthorizeAsync [Role] returns Succeeded")]
        public async Task AuthorizationManagerTest16Async()
        {
            var principal = new AppUser("TestUser@StEmilian.com");
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Create);

            var role = new AppRole { Name = "NewRole" };
            role.Claims.Add(
                new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = SysClaims.Role.List }
                );
            role.Claims.Add(
                new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = SysClaims.Role.Read }
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.RoleManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.RoleManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var authorizationManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);
            var result = await authorizationManager.AuthorizeAsync(claimsPrincipal, role, appClaimRequirement);

            Assert.True(result.Succeeded);
        }

        [Fact(DisplayName = "IsAuthorized returns True")]
        public async Task AuthorizationManagerTest17Async()
        {
            var role = new AppRole { Id = SysGuids.Role.UserManager, Name = nameof(SysGuids.Role.UserManager) };
            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(role.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);

            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { role.Id }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var result = await new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger)
                .IsAuthorizedAsync(claimsPrincipal, SysClaims.User.Read);

            Assert.True(result);
        }

        [Fact(DisplayName = "IsAuthorized returns False")]
        public async Task AuthorizationManagerTest18Async()
        {
            var role = new AppRole { Id = SysGuids.Role.UserManager, Name = nameof(SysGuids.Role.UserManager) };
            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(role.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);

            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { role.Id }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();

            var result = await new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger)
                .IsAuthorizedAsync(claimsPrincipal, SysClaims.Role.Read);

            Assert.False(result);
        }

        [Fact(DisplayName = "AppClaimRequirementProvider returns valid AuthorizationPolicy")]
        public async Task AuthorizationManagerTest19Async()
        {
            var attribute = new RequiresClaimsAttribute(SysClaims.Role.Read);

            var provider = new AppClaimRequirementProvider(
                new OptionsWrapper<AuthorizationOptions>(new AuthorizationOptions())
                );

            var result = await provider.GetPolicyAsync(attribute.Policy);

            Assert.Single(result.Requirements);
            Assert.Equal(result.Requirements[0].ToString(), attribute.ClaimValues[0]);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [User] fails delete request for system user")]
        public async Task AuthorizationManagerTest20Async()
        {
            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppUser("Administrator@StEmilian.com") { Id = SysGuids.User.Administrator };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Delete);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(appClaimRequirement.ToString(), logger.LogEntries[0].Message);
            Assert.Contains(typeof(AppUser).Name, logger.LogEntries[0].Message);
            Assert.Contains(resource.UserName, logger.LogEntries[0].Message);
            Assert.Contains(principal.UserName, logger.LogEntries[0].Message);
            Assert.Contains("restricted operation on system", logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [User] fails claim update request for system user")]
        public async Task AuthorizationManagerTest21Async()
        {
            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppUser("Administrator@StEmilian.com") { Id = SysGuids.User.Administrator };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.UpdateClaims);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(appClaimRequirement.ToString(), logger.LogEntries[0].Message);
            Assert.Contains(typeof(AppUser).Name, logger.LogEntries[0].Message);
            Assert.Contains(resource.UserName, logger.LogEntries[0].Message);
            Assert.Contains(principal.UserName, logger.LogEntries[0].Message);
            Assert.Contains("restricted operation on system", logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [User] allows update request for non-system user")]
        public async Task AuthorizationManagerTest22Async()
        {
            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.UserManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.User.DefinedClaims); }
                    )
                ).Returns(true);

            var role = new AppRole { Id = SysGuids.Role.UserManager, Name = nameof(SysGuids.Role.UserManager) };
            var dbContext = Mock.Of<AppDbContext>(db =>
                db.Set<AppRole>() == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IRepository<AppUser, AppRole>)) == dbContext &&
                sp.GetService(typeof(IHttpContextAccessor)) == new Mock<IHttpContextAccessor>().Object &&
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            Mock.Get(serviceProvider.GetRequiredService<IHttpContextAccessor>())
                .Setup(hca => hca.HttpContext.RequestServices).Returns(serviceProvider);

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppUser("ExistingUser@StEmilian.com");
            var appClaimRequirement = new AppClaimRequirement(SysClaims.User.Update);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [Role] fails delete request for system role")]
        public async Task AuthorizationManagerTest23Async()
        {
            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppRole() { Id = SysGuids.Role.RoleManager, Name = nameof(SysGuids.Role.RoleManager) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Delete);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(appClaimRequirement.ToString(), logger.LogEntries[0].Message);
            Assert.Contains(typeof(AppRole).Name, logger.LogEntries[0].Message);
            Assert.Contains(resource.Name, logger.LogEntries[0].Message);
            Assert.Contains(principal.UserName, logger.LogEntries[0].Message);
            Assert.Contains("restricted operation on system", logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [Role] fails claim update request for system role")]
        public async Task AuthorizationManagerTest24Async()
        {
            var principal = new AppUser("UserManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.UserManager }); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppRole() { Id = SysGuids.Role.RoleManager, Name = nameof(SysGuids.Role.RoleManager) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.UpdateClaims);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(appClaimRequirement.ToString(), logger.LogEntries[0].Message);
            Assert.Contains(typeof(AppRole).Name, logger.LogEntries[0].Message);
            Assert.Contains(resource.Name, logger.LogEntries[0].Message);
            Assert.Contains(principal.UserName, logger.LogEntries[0].Message);
            Assert.Contains("restricted operation on system", logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [Role] allows update request for non-system role")]
        public async Task AuthorizationManagerTest25Async()
        {
            var principal = new AppUser("RoleManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.RoleManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.RoleManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppRole() { Name = "ExistingRole" };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Update);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.True(authContext.HasSucceeded);
        }

        [Fact(DisplayName = "AppClaimRequirementHandler [Role] only returns failing claims.")]
        public async Task AuthorizationManagerTest26Async()
        {
            var principal = new AppUser("RoleManager@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
                cp.Identity.Name == principal.UserName
                );

            var userRoleCache = new Mock<UserRoleCache>();
            userRoleCache.Setup(mc => mc.TryGetValue(principal.Id, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(new[] { SysGuids.Role.RoleManager }); }
                    )
                ).Returns(true);

            var roleClaimCache = new Mock<RoleClaimCache>();
            roleClaimCache.Setup(mc => mc.TryGetValue(SysGuids.Role.RoleManager, out It.Ref<object>.IsAny))
                .Callback(new TryGetValueDelegate(
                    (object key, out object value) => { value = new HashSet<string>(SysClaims.Role.DefinedClaims); }
                    )
                ).Returns(true);

            var serviceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(UserRoleCache)) == userRoleCache.Object &&
                sp.GetService(typeof(RoleClaimCache)) == roleClaimCache.Object
                );

            var logger = new TestLogger<AuthorizationManager>();
            var authManager = new AuthorizationManager<AppUser, AppRole>(serviceProvider, logger);

            var resource = new AppRole() { Id = SysGuids.Role.UserManager, Name = nameof(SysGuids.Role.UserManager) };
            var appClaimRequirement = new AppClaimRequirement(SysClaims.Role.Read, SysClaims.Role.UpdateClaims);
            var authContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { appClaimRequirement }, claimsPrincipal, resource);

            await new AppClaimRequirementHandler(authManager)
                .HandleAsync(authContext);

            Assert.False(authContext.HasSucceeded);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.DoesNotContain(SysClaims.Role.Read, logger.LogEntries[0].Message);
        }
    }
}