using Authorization.Core.UI.Tests.Data;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using CRFricke.Authorization.Core.UI.Pages.V5.User;
using CRFricke.Test.Support.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests;

public class UserManagementTests : TestsBase
{
    private static UserManager<ApplicationUser> CreateUserManagerMock(
        IUserStore<ApplicationUser> userStore = null,
        IOptions<IdentityOptions> optionsAccessor = null,
        IPasswordHasher<ApplicationUser> passwordHasher = null,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators = null,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators = null,
        ILookupNormalizer keyNormalizer = null,
        IdentityErrorDescriber errors = null,
        IServiceProvider services = null
        )
    {
        var userMock = new Mock<UserManager<ApplicationUser>>(
            userStore ?? Mock.Of<IUserStore<ApplicationUser>>(),
            optionsAccessor,
            passwordHasher ?? Mock.Of<IPasswordHasher<ApplicationUser>>(),
            userValidators,
            passwordValidators,
            keyNormalizer ?? Mock.Of<ILookupNormalizer>(),
            errors ?? Mock.Of<IdentityErrorDescriber>(),
            services,
            new FakeLogger<UserManager<ApplicationUser>>()
            );

        return userMock.Object;
    }

    private static List<ApplicationRole> GetDefinedRoles()
    {
        return
        [
            new() { Id = SysGuids.Role.Administrator, Name = nameof(SysGuids.Role.Administrator) },
            new() { Id = SysUiGuids.Role.RoleManager, Name = nameof(SysUiGuids.Role.RoleManager) },
            new() { Id = SysUiGuids.Role.UserManager, Name = nameof(SysUiGuids.Role.UserManager) }
        ];
    }

    [Fact(DisplayName = "UserManagement page returns list of Users")]
    public async Task UserManagement_Test1Async()
    {
        var users = new List<ApplicationUser>
        {
            new ApplicationUser("Admin@company.com"),
            new ApplicationUser("TestUser@company.com") { GivenName = "Test", Surname = "User"   }
        };
        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var model = new IndexModel<ApplicationUser, ApplicationRole>(repository);
        await model.OnGetAsync();

        Assert.Equal(2, model.UserInfo.Count);
        Assert.Equal(users[0].Email, model.UserInfo[0].Email);
        Assert.Equal(users[1].Email, model.UserInfo[1].Email);
    }

    [Fact(DisplayName = "Create User [Get] initializes the RoleInfo collection")]
    public async void UserManagement_Test2()
    {
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = "Administrator" },
            new ApplicationRole { Name = "RoleManager" },
            new ApplicationRole { Name = "UserManager" }
        };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedClaims == GetDefinedClaims() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var userManager = CreateUserManagerMock();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository, logger);
        _ = await model.OnGetAsync();

        Assert.Equal(3, model.UserModel.Roles.Count);
        Assert.Equal(roles[0].Name, model.UserModel.Roles.First().Name);
        Assert.Equal(roles[2].Name, model.UserModel.Roles.Last().Name);
    }

    [Fact(DisplayName = "Create User [Post] sets assigned role claims")]
    public async Task UserManagement_Test3Async()
    {
        ApplicationUser user = null;
        var expectedClaims = new string[] { SysUiGuids.Role.RoleManager };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedClaims == GetDefinedClaims() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var userManager = CreateUserManagerMock();

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Roles).Returns(GetDefinedRoles().AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Add(It.IsAny<ApplicationUser>()))
            .Callback((ApplicationUser au) => { user = au; })
            .Returns((EntityEntry<ApplicationUser>)null);

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository.Object, logger)
        {
            UserModel = new UserModel { Email = "TestUser@company.com", Password = "MyStrongPassword" },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        await model.OnPostAsync(string.Join(',', expectedClaims));

        Assert.NotNull(user);
        Assert.Equal(expectedClaims.Length, user.Claims.Count);
        Assert.Equal(expectedClaims, user.Claims.Select(c => c.ClaimValue));
    }

    [Fact(DisplayName = "Create User [Post] handles DB exception")]
    public async Task UserManagement_Test4Async()
    {
        ApplicationUser user = null;
        var dbUpdateException =
            new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
            new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationUser_Id'.")
            );

        var roles = new List<ApplicationRole>();

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedClaims == GetDefinedClaims() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var userManager = CreateUserManagerMock();

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Roles).Returns(roles.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Add(It.IsAny<ApplicationUser>()))
            .Callback((ApplicationUser au) => { user = au; })
            .Throws(dbUpdateException);

        var passwordHasher = Mock.Of<IPasswordHasher<ApplicationUser>>();

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository.Object, logger)
        {
            UserModel = new UserModel { Email = "TestUser@company.com", Password = "MyStrongPassword" },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains("User", errors[0].ErrorMessage);
        Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.NotNull(logger.LatestRecord.Exception);
    }

    [Fact(DisplayName = "Create User [Post] sends notification on success")]
    public async Task UserManagement_Test5Async()
    {
        ApplicationUser user = null;

        var roles = new List<ApplicationRole>();

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedClaims == GetDefinedClaims() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var userManager = CreateUserManagerMock();

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Roles).Returns(roles.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Add(It.IsAny<ApplicationUser>()))
            .Callback((ApplicationUser au) => { user = au; })
            .Returns((EntityEntry<ApplicationUser>)null);

        var passwordHasher = Mock.Of<IPasswordHasher<ApplicationUser>>();

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository.Object, logger)
        {
            UserModel = new UserModel { Email = "TestUser@company.com", Password = "MyStrongPassword" },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.Single(model.TempData);
        var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
        Assert.Contains(user.Email, notifications[0].Message);
    }

    [Fact(DisplayName = "Create User [Post] logs success")]
    public async Task UserManagement_Test6Async()
    {
        ApplicationUser user = null;

        var roles = new List<ApplicationRole>();

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedClaims == GetDefinedClaims() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var userManager = CreateUserManagerMock();

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Roles).Returns(roles.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Add(It.IsAny<ApplicationUser>()))
            .Callback((ApplicationUser au) => { user = au; })
            .Returns((EntityEntry<ApplicationUser>)null);

        var passwordHasher = Mock.Of<IPasswordHasher<ApplicationUser>>();

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository.Object, logger)
        {
            UserModel = new UserModel { Email = "TestUser@company.com", Password = "MyStrongPassword" },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        await model.OnPostAsync(string.Empty);

        Assert.NotNull(user);
        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Information, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains($"created {nameof(ApplicationUser)}", logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
    }

    [Fact(DisplayName = "Edit User [Get] returns NotFound for null ID")]
    public async Task UserManagement_Test7Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();
        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>();
        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        var result = await model.OnGetAsync(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Edit User [Get] initializes ApplicationUserModel")]
    public async Task UserManagement_Test8Async()
    {
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var expectedClaims = new[] { roles[2].Name };

        var user = new ApplicationUser("TestUser@company.com")
        {
            AccessFailedCount = 1,
            EmailConfirmed = true,
            GivenName = "Test",
            Surname = "User",
            LockoutEnabled = true,
            LockoutEnd = new DateTimeOffset(DateTime.Now),
            PhoneNumber = "123-456-7890",
            PhoneNumberConfirmed = true,
        };
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = expectedClaims[0] }
            );
        var users = new List<ApplicationUser> { user };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedGuids == GetDefinedGuids() &&
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        var result = await model.OnGetAsync(user.Id);

        Assert.Equal(user.Id, model.UserModel.Id);
        Assert.Equal(user.AccessFailedCount, model.UserModel.AccessFailedCount);
        Assert.Equal(user.Email, model.UserModel.Email);
        Assert.Equal(user.EmailConfirmed, model.UserModel.EmailConfirmed);
        Assert.Equal(user.GivenName, model.UserModel.GivenName);
        Assert.Equal(user.LockoutEnabled, model.UserModel.LockoutEnabled);
        Assert.Equal(user.LockoutEnd, model.UserModel.LockoutEndUtc);
        Assert.Equal(user.PhoneNumber, model.UserModel.PhoneNumber);
        Assert.Equal(user.PhoneNumberConfirmed, model.UserModel.PhoneNumberConfirmed);
        Assert.Equal(user.Surname, model.UserModel.Surname);

        var claims = model.UserModel.Roles.Where(ri => ri.IsAssigned);
        Assert.Equal(expectedClaims.Length, user.Claims.Count);
        Assert.Equal(expectedClaims, user.Claims.Select(c => c.ClaimValue));
    }

    [Fact(DisplayName = "Edit User [Post] sends notification for DB not found")]
    public async Task UserManagement_Test9Async()
    {
        var users = new List<ApplicationUser>();
        var roles = new List<ApplicationRole>();

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = new UserModel { Email = "TestUser@company.com" },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(model.TempData);
        var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
        Assert.Contains(model.UserModel.Email, notifications[0].Message);
    }

    [Fact(DisplayName = "Edit User [Post] handles DB exception")]
    public async Task UserManagement_Test10Async()
    {
        var dbUpdateException =
            new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
            new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationRole_Id'.")
            );

        var users = new List<ApplicationUser> { new ApplicationUser("TestUser@company.com") };
        var roles = new List<ApplicationRole> { };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Roles).Returns(roles.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.SaveChangesAsync(default)).Throws(dbUpdateException);

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
        {
            UserModel = new UserModel { Id = users[0].Id, Email = "TestUser@company.com" },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains("User", errors[0].ErrorMessage);
        Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(users[0].Id, logger.LatestRecord.Message);
        Assert.Contains(users[0].Email, logger.LatestRecord.Message);
        Assert.NotNull(logger.LatestRecord.Exception);
    }

    [Fact(DisplayName = "Edit User [Post] sends no notification for no changes")]
    public async Task UserManagement_Test11Async()
    {
        var users = new List<ApplicationUser> { new ApplicationUser("TestUser@company.com") };
        var roles = new List<ApplicationRole>();

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object &&
            db.SaveChangesAsync(default) == Task.FromResult(0)
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = new UserModel { Id = users[0].Id },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Empty(model.TempData);
    }

    [Fact(DisplayName = "Edit User [Post] updates User properties")]
    public async Task UserManagement_Test12Async()
    {
        var expectedClaims = new string[] { SysUiGuids.Role.RoleManager, SysUiGuids.Role.UserManager };

        var user = new ApplicationUser("OldUser@company.com");
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = SysGuids.Role.Administrator }
            );
        var users = new List<ApplicationUser> { user };

        var expectedValue = new Dictionary<string, string>
        {
            { nameof(user.ConcurrencyStamp), user.ConcurrencyStamp },
            { nameof(user.Id), user.Id },
            { nameof(user.SecurityStamp), user.SecurityStamp }
        };

        var userModel = new UserModel
        {
            Id = user.Id,
            AccessFailedCount = 1,
            Email = "NewUser@company.com",
            EmailConfirmed = true,
            GivenName = "Test",
            LockoutEnabled = true,
            LockoutEndUtc = new DateTimeOffset(DateTime.Now),
            PhoneNumber = "123-456-7890",
            PhoneNumberConfirmed = true,
            Surname = "User"
        };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == GetDefinedRoles().AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object &&
            db.SaveChangesAsync(default) == Task.FromResult(0)
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = userModel,
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Join(',', expectedClaims));

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(userModel.AccessFailedCount, user.AccessFailedCount);
        Assert.Equal(userModel.Email, user.Email);
        Assert.Equal(userModel.Email.ToUpperInvariant(), user.NormalizedEmail);
        Assert.Equal(userModel.EmailConfirmed, user.EmailConfirmed);
        Assert.Equal(userModel.GivenName, user.GivenName);
        Assert.Equal(userModel.LockoutEnabled, user.LockoutEnabled);
        Assert.Equal(userModel.LockoutEndUtc, user.LockoutEnd);
        Assert.Equal(userModel.PhoneNumber, user.PhoneNumber);
        Assert.Equal(userModel.PhoneNumberConfirmed, user.PhoneNumberConfirmed);
        Assert.Equal(userModel.Surname, user.Surname);

        Assert.Equal(expectedValue[nameof(user.ConcurrencyStamp)], user.ConcurrencyStamp);
        Assert.Equal(expectedValue[nameof(user.Id)], user.Id);
        Assert.Equal(expectedValue[nameof(user.SecurityStamp)], user.SecurityStamp);

        Assert.Equal(expectedClaims.Length, user.Claims.Count);
        Assert.Equal(expectedClaims, user.Claims.Select(c => c.ClaimValue));
    }

    [Fact(DisplayName = "Edit User [Post] sends notification for successful update")]
    public async Task UserManagement_Test13Async()
    {
        var roles = new List<ApplicationRole> { };

        var user = new ApplicationUser("TestUser@company.com");
        var users = new List<ApplicationUser> { user };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object &&
            db.SaveChangesAsync(default) == Task.FromResult(1)
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = new UserModel { Id = user.Id, Email = user.Email },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.Single(model.TempData);
        var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
        Assert.Contains(user.Email, notifications[0].Message);
    }

    [Fact(DisplayName = "Edit User [Post] logs successful update")]
    public async Task UserManagement_Test14Async()
    {
        var roles = new List<ApplicationRole> { };

        var user = new ApplicationUser("TestUser@company.com");
        var users = new List<ApplicationUser> { user };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object &&
            db.SaveChangesAsync(default) == Task.FromResult(1)
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = new UserModel { Id = user.Id, Email = user.Email },
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        await model.OnPostAsync(string.Empty);

        Assert.NotNull(user);
        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Information, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
    }

    [Fact(DisplayName = "Display User returns NotFound for null ID")]
    public async Task UserManagement_Test15Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();
        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>();

        var model = new DetailsModel<ApplicationUser, ApplicationRole>(authManager, repository);
        var result = await model.OnGetAsync(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Display User returns NotFound for DB not found")]
    public async Task UserManagement_Test16Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == new List<ApplicationUser>().AsQueryable().BuildMockDbSet().Object 
            );

        var model = new DetailsModel<ApplicationUser, ApplicationRole>(authManager, repository);
        var result = await model.OnGetAsync(Guid.Empty.ToString());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Display User initializes ApplicationUserModel")]
    public async Task UserManagement_Test17Async()
    {
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var expectedClaims = new[] { roles[2].Name };

        var user = new ApplicationUser("TestUser@company.com")
        {
            AccessFailedCount = 1,
            EmailConfirmed = true,
            GivenName = "Test",
            Surname = "User",
            LockoutEnabled = true,
            LockoutEnd = new DateTimeOffset(DateTime.Now),
            PhoneNumber = "123-456-7890",
            PhoneNumberConfirmed = true,
        };
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = expectedClaims[0] }
            );
        var users = new List<ApplicationUser> { user };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedGuids == GetDefinedGuids()
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var model = new DetailsModel<ApplicationUser, ApplicationRole>(authManager, repository);
        var result = await model.OnGetAsync(user.Id);

        Assert.Equal(user.Id, model.UserModel.Id);
        Assert.Equal(user.AccessFailedCount, model.UserModel.AccessFailedCount);
        Assert.Equal(user.Email, model.UserModel.Email);
        Assert.Equal(user.EmailConfirmed, model.UserModel.EmailConfirmed);
        Assert.Equal(user.GivenName, model.UserModel.GivenName);
        Assert.Equal(user.LockoutEnabled, model.UserModel.LockoutEnabled);
        Assert.Equal(user.LockoutEnd, model.UserModel.LockoutEndUtc);
        Assert.Equal(user.PhoneNumber, model.UserModel.PhoneNumber);
        Assert.Equal(user.PhoneNumberConfirmed, model.UserModel.PhoneNumberConfirmed);
        Assert.Equal(user.Surname, model.UserModel.Surname);

        var claims = model.UserModel.Roles.Where(ri => ri.IsAssigned);
        Assert.Equal(expectedClaims.Length, user.Claims.Count);
        Assert.Equal(expectedClaims, user.Claims.Select(c => c.ClaimValue));
    }

    [Fact(DisplayName = "Delete User [Get] returns NotFound for null ID")]
    public async Task UserManagement_Test18Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();
        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>();
        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        var result = await model.OnGetAsync(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Delete User [Get] returns NotFound for DB not found")]
    public async Task UserManagement_Test19Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == new List<ApplicationUser>().AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        var result = await model.OnGetAsync(Guid.Empty.ToString());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Delete User [Get] initializes RoleInfo collection")]
    public async Task UserManagement_Test20Async()
    {
        var roles = GetDefinedRoles();

        var expectedClaims = new[] { SysUiGuids.Role.RoleManager };

        //
        // MockQueryable does not support .Include(au => au.Claims) so we add the claims manually.
        //
        var users = new List<ApplicationUser>
        {
            new ApplicationUser("TestUser1@company.com").SetClaims(SysGuids.Role.Administrator),
            new ApplicationUser("TestUser2@company.com").SetClaims(expectedClaims)
        };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedGuids == GetDefinedGuids()
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        await model.OnGetAsync(users[1].Id);

        Assert.Equal(roles.Count, model.UserModel.Roles.Count);

        var claims = model.UserModel.Roles.Where(ri => ri.IsAssigned);
        Assert.Equal(expectedClaims.Length, claims.Count());
        Assert.Equal(expectedClaims, claims.Select(c => c.Id));
    }

    [Fact(DisplayName = "Delete User [Post] returns NotFound for null ID")]
    public async Task UserManagement_Test21Async()
    {
        var authManager = Mock.Of<IAuthorizationManager>();
        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>();
        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger);
        var result = await model.OnPostAsync(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Delete User [Post] sends notification on DB not found")]
    public async Task UserManagement_Test22Async()
    {
        var user = new ApplicationUser("TestUser@company.com");
        var users = new List<ApplicationUser>(new[] { user });

        var authManager = Mock.Of<IAuthorizationManager>();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(Guid.Empty.ToString());

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(model.TempData);
        var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
        Assert.Contains(user.Email, notifications[0].Message);
    }

    [Fact(DisplayName = "Delete User [Post] handles DB exception")]
    public async Task UserManagement_Test23Async()
    {
        var dbUpdateException =
            new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
            new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationRole_Id'.")
            );

        var user = new ApplicationUser("TestUser@company.com");

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Users).Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Remove(It.IsAny<ApplicationUser>())).Throws(dbUpdateException);
        repository.Setup(db => db.Roles).Returns(new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object);

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(user.Id);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains("User", errors[0].ErrorMessage);
        Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Error, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
        Assert.NotNull(logger.LatestRecord.Exception);
    }

    [Fact(DisplayName = "Delete User [Post] sends notification for delete")]
    public async Task UserManagement_Test24Async()
    {
        var user = new ApplicationUser("TestUser@company.com");

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object &&
            db.Users.Remove(It.IsAny<ApplicationUser>()) == (EntityEntry<ApplicationUser>)null &&
            db.SaveChangesAsync(default) == Task.FromResult(1)
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(user.Id);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Single(model.TempData);
        var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
        Assert.Contains(user.Email, notifications[0].Message);
    }

    [Fact(DisplayName = "Delete User [Post] logs successful delete")]
    public async Task UserManagement_Test25Async()
    {
        ApplicationUser deletedUser = null;

        var user = new ApplicationUser("TestUser@company.com");

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
        repository.Setup(db => db.Users).Returns(new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object);
        repository.Setup(db => db.Users.Remove(It.IsAny<ApplicationUser>()))
            .Callback((ApplicationUser au) => { deletedUser = au; })
            .Returns((EntityEntry<ApplicationUser>)null);
        repository.Setup(db => db.SaveChangesAsync(default)).Returns(Task.FromResult(1));

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(user.Id);

        Assert.IsType<RedirectToPageResult>(result);

        Assert.NotNull(deletedUser);
        Assert.True(ReferenceEquals(user, deletedUser));

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Information, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
    }

    [Fact(DisplayName = "Delete User [Post] prevents delete of System User")]
    public async Task UserManagement_Test26Async()
    {
        var expectedMessage = "System accounts may not be deleted";

        var user = new ApplicationUser("TestUser@company.com");
        var users = new List<ApplicationUser>(new[] { user });

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), user, It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.SystemObject(null))
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object &&
            db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(user.Id);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains(expectedMessage, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains("delete system", logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
    }

    [Fact(DisplayName = "Edit User [Post] prevents update of System User")]
    public async Task UserManagement_Test27Async()
    {
        var expectedMessage = "You may not update the Roles assigned to a system User";
        var expectedLogMessage = $"update the Roles of system {nameof(ApplicationUser)}";

        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var expectedClaims = new string[] { roles[1].Name, roles[2].Name };

        var user = new ApplicationUser("TestUser@company.com");
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = roles[0].Name }
            );
        var users = new List<ApplicationUser> { user };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), user, It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.SystemObject(null))
            );

        var dbContext = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, dbContext, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Join(',', expectedClaims));

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains(expectedMessage, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(expectedLogMessage, logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
    }

    [Fact(DisplayName = "Edit User [Post] refreshes User cache on success")]
    public async Task UserManagement_Test28Async()
    {
        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var expectedClaims = new string[] { roles[1].Name, roles[2].Name };

        var user = new ApplicationUser("TestUser@company.com");
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = roles[0].Name }
            );
        var users = new List<ApplicationUser> { user };

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), user, It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object &&
            db.SaveChangesAsync(default) == Task.FromResult(1)
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        string cacheKey = null;
        Mock.Get(authManager).Setup(am => am.RefreshUser(user.Id)).Callback((string id) => { cacheKey = id; });

        await model.OnPostAsync(string.Join(',', expectedClaims));

        Assert.Equal(user.Id, cacheKey);
    }

    [Fact(DisplayName = "Delete User [Post] refreshes User cache on success")]
    public async Task UserManagement_Test29Async()
    {
        var user = new ApplicationUser("TestUser@company.com");

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), user, It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Users == new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet().Object &&
            db.Users.Remove(user) == null &&
            db.SaveChangesAsync(default) == Task.FromResult(1)
            );

        var logger = new FakeLogger<DeleteHandler>();

        var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        string cacheKey = null;
        Mock.Get(authManager).Setup(am => am.RefreshUser(user.Id)).Callback((string id) => { cacheKey = id; });

        await model.OnPostAsync(user.Id);

        Assert.Equal(user.Id, cacheKey);
    }

    [Fact(DisplayName = "Edit User [Get] sets IsSystemUser")]
    public async Task UserManagement_Test30Async()
    {
        var roles = new List<ApplicationRole>();

        var user = new ApplicationUser { Id = SysGuids.User.Administrator, Email = "Admin@company.com" };
        var users = new List<ApplicationUser> { user };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.DefinedGuids == GetDefinedGuids()
            );

        var dbContext = Mock.Of<ApplicationDbContext>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, dbContext, logger);
        await model.OnGetAsync(user.Id);

        Assert.True(model.UserModel.IsSystemUser);
    }

    [Fact(DisplayName = "Create User [Post] handles failed elevation check")]
    public async Task UserManagement_Test31Async()
    {
        var expectedMessage = "can not create a User with more privileges than you";
        var expectedLogMessage = $"create {nameof(ApplicationUser)} with elevated privileges";

        var user = new ApplicationUser("NewUser@company.com");

        var principalId = Guid.NewGuid().ToString();
        var principalName = "TestUser@company.com";
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalId) } &&
            cp.Identity.Name == principalName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Elevation(null))
            );

        var userManager = CreateUserManagerMock();

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == Enumerable.Empty<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
            );

        var passwordHasher = Mock.Of<IPasswordHasher<ApplicationUser>>();

        var logger = new FakeLogger<CreateHandler>();

        var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, userManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains(expectedMessage, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(principalName, logger.LatestRecord.Message);
        Assert.Contains(expectedLogMessage, logger.LatestRecord.Message);
        Assert.Null(logger.LatestRecord.Exception);
    }

    [Fact(DisplayName = "Edit User [Post] handles failed elevation check")]
    public async Task UserManagement_Test32Async()
    {
        var expectedMessage = "can not give a User more privileges than you";
        var expectedLogMessage = "elevated privileges";

        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var principalUser = new ApplicationUser("AdminUser@company.com");
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principalUser.Id) } &&
            cp.Identity.Name == principalUser.UserName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var user = new ApplicationUser("TestUser@company.com");
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = roles[0].Name }
            );
        var users = new List<ApplicationUser> { user };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), user, It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Elevation(null))
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains(expectedMessage, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(principalUser.UserName, logger.LatestRecord.Message);
        Assert.Contains(nameof(ApplicationUser), logger.LatestRecord.Message);
        Assert.Contains(user.Email, logger.LatestRecord.Message);
        Assert.Contains(user.Id, logger.LatestRecord.Message);
        Assert.Contains(expectedLogMessage, logger.LatestRecord.Message);
        Assert.Null(logger.LatestRecord.Exception);
    }

    [Fact(DisplayName = "Edit User [Post] handles elevation of self")]
    public async Task UserManagement_Test33Async()
    {
        var expectedMessage = "can not elevate your own privileges";
        var expectedLogMessage = "attempted to elevate their own privileges";

        var roles = new List<ApplicationRole>
        {
            new ApplicationRole { Name = nameof(SysGuids.Role.Administrator) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.RoleManager) },
            new ApplicationRole { Name = nameof(SysUiGuids.Role.UserManager) }
        };

        var principal = new ApplicationUser("AdminUser@company.com");
        var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
            cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, principal.Id) } &&
            cp.Identity.Name == principal.UserName
            );

        var httpContext = Mock.Of<HttpContext>(hc =>
            hc.User == claimsPrincipal
            );

        var user = new ApplicationUser("TestUser@company.com") { Id = principal.Id };
        user.Claims.Add(
            new IdentityUserClaim<string> { Id = 1, UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = roles[0].Name }
            );
        var users = new List<ApplicationUser> { user };

        var authManager = Mock.Of<IAuthorizationManager>(am =>
            am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationUser>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Elevation(null))
            );

        var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
            db.Roles == roles.AsQueryable().BuildMockDbSet().Object &&
            db.Users == users.AsQueryable().BuildMockDbSet().Object
            );

        var logger = new FakeLogger<EditHandler>();

        var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
        {
            UserModel = CreateModelFromUser(user),
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TestTempDataDictionary()
        };

        var result = await model.OnPostAsync(string.Empty);

        Assert.IsType<PageResult>(result);

        Assert.False(model.ModelState.IsValid);
        Assert.Equal(2, model.ModelState.ErrorCount);
        var errors = model.ModelState[string.Empty].Errors;
        Assert.Contains(expectedMessage, errors[1].ErrorMessage);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
        Assert.Contains(principal.UserName, logger.LatestRecord.Message);
        Assert.Contains(expectedLogMessage, logger.LatestRecord.Message);
        Assert.Null(logger.LatestRecord.Exception);
    }
}
