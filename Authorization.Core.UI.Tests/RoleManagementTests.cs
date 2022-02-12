using Authorization.Core.UI.Tests.Data;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using CRFricke.Authorization.Core.UI.Pages.V4.Role;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Test.Support.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Authorization.Core.UI.Tests
{
    [Collection("Non-Parallel Tests")]
    [CollectionDefinition("Non-Parallel Tests", DisableParallelization = true)]
    public class NonParallelTests
    {
        [Fact(DisplayName = "Create Role [Get] initializes RoleClaim collection")]
        public void RoleManagement_Test2()
        {
            var authManager = (IAuthorizationManager)new AuthorizationManager<ApplicationUser, ApplicationRole>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, null, null);
            model.OnGet();

            Assert.Equal(authManager.DefinedClaims.Count, model.RoleModel.RoleClaims.Count);
        }
    }

    public class RoleManagementTests : TestsBase
    {
        [Fact(DisplayName = "RoleManagement page returns list of ApplicationRoles")]
        public async Task RoleManagement_Test1Async()
        {
            var roles = new List<ApplicationRole>
            {
                new ApplicationRole { Name = "Administrator", Description = "Anyone with this role can do anything!" },
                new ApplicationRole { Name = "TestRole", Description = "Used for testing." }
            };
            var dbSet = roles.AsQueryable().BuildMockDbSet();
            var dbContext = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object
                );

            var model = new IndexModel<ApplicationUser, ApplicationRole>(dbContext);
            await model.OnGetAsync();

            Assert.Equal(2, model.Roles.Count);
            Assert.Equal(roles[0].Name, model.Roles[0].Name);
            Assert.Equal(roles[1].Name, model.Roles[1].Name);
        }

        [Fact(DisplayName = "Create Role [Post] sets assigned claims")]
        public async Task RoleManagement_Test3Async()
        {
            ApplicationRole role = null;
            var expectedClaims = new string[] { SysClaims.Role.Create, SysClaims.Role.Update };

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.Add(It.IsAny<ApplicationRole>()))
                .Callback((ApplicationRole ar) => { role = ar; })
                .Returns((EntityEntry<ApplicationRole>)null);

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Join(',', expectedClaims));

            Assert.NotNull(role);
            Assert.Equal(expectedClaims.Length, role.Claims.Count);
            Assert.Equal(expectedClaims, role.Claims.Select(c => c.ClaimValue));
        }

        [Fact(DisplayName = "Create Role [Post] sets ApplicationRole properties")]
        public async Task RoleManagement_Test4Async()
        {
            ApplicationRole role = null;
            var name = "TestRole";
            var description = "Can do tester stuff.";

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>> ();
            repository.Setup(db => db.Roles.Add(It.IsAny<ApplicationRole>()))
                .Callback((ApplicationRole ar) => { role = ar; })
                .Returns((EntityEntry<ApplicationRole>)null);

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = new RoleModel { Name = name, Description = description },
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Assert.NotNull(role);
            Assert.Equal(name, role.Name);
            Assert.Equal(description, role.Description);
            Assert.Equal(name.ToUpperInvariant(), role.NormalizedName);
        }

        [Fact(DisplayName = "Create Role [Post] handles DB exception")]
        public async Task RoleManagement_Test5Async()
        {
            var dbUpdateException =
                new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
                new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationRole_Id'.")
                );

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.Add(It.IsAny<ApplicationRole>())).Throws(dbUpdateException);

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." }
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains("Role", errors[0].ErrorMessage);
            Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Error, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.NotNull(logger.LogEntries[0].Exception);
        }

        [Fact(DisplayName = "Create Role [Post] logs success")]
        public async Task RoleManagement_Test6Async()
        {
            ApplicationRole role = null;

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.Add(It.IsAny<ApplicationRole>()))
                .Callback((ApplicationRole ar) => { role = ar; })
                .Returns((EntityEntry<ApplicationRole>)null);

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Assert.NotNull(role);
            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "Create Role [Post] sends notification on success")]
        public async Task RoleManagement_Test7Async()
        {
            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles.Add(It.IsAny<ApplicationRole>()) == null
                );

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Assert.Equal(1, model.TempData.Count);
            var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
            Assert.Contains(model.RoleModel.Name, notifications[0].Message);
        }

        [Fact(DisplayName = "Edit Role [Get] returns NotFound for null ID")]
        public async Task RoleManagement_Test8Async()
        {
            var model = new EditModel<ApplicationUser, ApplicationRole> (null, null, null);
            var result = await model.OnGetAsync(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Edit Role [Get] returns NotFound for DB not found")]
        public async Task RoleManagement_Test9Async()
        {
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
                );

            var model = new EditModel<ApplicationUser, ApplicationRole>(null, repository, null);
            var result = await model.OnGetAsync(Guid.Empty.ToString());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Edit Role [Get] initializes RoleModel")]
        public async Task RoleManagement_Test10Async()
        {
            var expectedValue = "Role.List";

            var roles = new List<ApplicationRole>
            {
                new ApplicationRole { Name = "TestRole", Description = "Used for testing." }
            };

            roles[0].Claims.Add(
                new IdentityRoleClaim<string> { RoleId = roles[0].Id, ClaimType = ClaimTypes.AuthorizationDecision, ClaimValue = expectedValue }
                );

            var definedClaims = GetDefinedClaims();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedGuids == GetDefinedGuids() &&
                am.DefinedClaims == definedClaims
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == roles.AsQueryable().BuildMockDbSet().Object
                );

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, null);
            await model.OnGetAsync(roles[0].Id);

            Assert.Equal(roles[0].Id, model.RoleModel.Id);
            Assert.Equal(roles[0].Description, model.RoleModel.Description);
            Assert.Equal(roles[0].Name, model.RoleModel.Name);
            Assert.Equal(definedClaims.Count, model.RoleModel.RoleClaims.Count);

            var claims = model.RoleModel.RoleClaims.Where(rc => rc.IsAssigned);
            Assert.Single(claims);
            Assert.Equal(expectedValue, claims.First().Claim);
        }

        [Fact(DisplayName = "Edit Role [Post] sends notification for DB not found")]
        public async Task RoleManagement_Test11Async()
        {
            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
                );

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, null)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(1, model.TempData.Count);
            var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
            Assert.Contains(model.RoleModel.Name, notifications[0].Message);
        }

        [Fact(DisplayName = "Edit Role [Post] handles DB exception")]
        public async Task RoleManagement_Test12Async()
        {
            var dbUpdateException =
                new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
                new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationRole_Id'.")
                );

            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles).Returns(dbSet.Object);
            repository.Setup(db => db.SaveChangesAsync(default)).Throws(dbUpdateException);

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains("Role", errors[0].ErrorMessage);
            Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Error, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
            Assert.NotNull(logger.LogEntries[0].Exception);
        }

        [Fact(DisplayName = "Edit Role [Post] sends no notification for no changes")]
        public async Task RoleManagement_Test13Async()
        {
            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(0)
                );

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, null)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(0, model.TempData.Count);
        }

        [Fact(DisplayName = "Edit Role [Post] sets ApplicationRole properties")]
        public async Task RoleManagement_Test14Async()
        {
            var expectedName = "TestManager";
            var expectedDescription = "TestManagers do things related to testing.";

            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = new RoleModel { Id = role.Id, Name = expectedName, Description = expectedDescription },
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(expectedDescription, role.Description);
            Assert.Equal(expectedName, role.Name);
            Assert.Equal(expectedName.ToUpperInvariant(), role.NormalizedName);
        }

        [Fact(DisplayName = "Edit Role [Post] updates RoleClaims")]
        public async Task RoleManagement_Test15Async()
        {
            var expectedClaims = new string[] { SysClaims.Role.Create, SysClaims.Role.Update };

            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            role.Claims.Add(new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = ClaimTypes.AuthorizationDecision, ClaimValue = SysClaims.User.Create });
            role.Claims.Add(new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = ClaimTypes.AuthorizationDecision, ClaimValue = expectedClaims[0] });

            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Join(',', expectedClaims));

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(expectedClaims.Length, role.Claims.Count);
            Assert.NotNull(role.Claims.FirstOrDefault(c => c.ClaimValue == expectedClaims[0]));
            Assert.NotNull(role.Claims.FirstOrDefault(c => c.ClaimValue == expectedClaims[1]));
        }

        [Fact(DisplayName = "Edit Role [Post] sends notification for update")]
        public async Task RoleManagement_Test16Async()
        {
            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Assert.Equal(1, model.TempData.Count);
            var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
            Assert.Contains(model.RoleModel.Name, notifications[0].Message);
        }

        [Fact(DisplayName = "Edit Role [Post] logs success")]
        public async Task RoleManagement_Test17Async()
        {
            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "Display Role returns NotFound for null ID")]
        public async Task RoleManagement_Test18Async()
        {
            var model = new DetailsModel<ApplicationUser, ApplicationRole>(null, null);
            var result = await model.OnGetAsync(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Display Role returns NotFound for DB not found")]
        public async Task RoleManagement_Test19Async()
        {
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
                );

            var model = new DetailsModel<ApplicationUser, ApplicationRole>(null, repository);
            var result = await model.OnGetAsync(Guid.Empty.ToString());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Display Role initializes RoleModel")]
        public async Task RoleManagement_Test20Async()
        {
            var expectedValue = "Role.List";

            var roles = new List<ApplicationRole>
            {
                new ApplicationRole { Name = "TestRole", Description = "Used for testing." }
            };

            roles[0].Claims.Add(
                new IdentityRoleClaim<string> { RoleId = roles[0].Id, ClaimType = ClaimTypes.AuthorizationDecision, ClaimValue = expectedValue }
                );

            var definedClaims = GetDefinedClaims();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == definedClaims
                );

            var dbContext = Mock.Of<ApplicationDbContext>(db =>
                db.Roles == roles.AsQueryable().BuildMockDbSet().Object
                );

            var model = new DetailsModel<ApplicationUser, ApplicationRole>(authManager, dbContext);
            await model.OnGetAsync(roles[0].Id);

            Assert.Equal(roles[0].Id, model.RoleModel.Id);
            Assert.Equal(roles[0].Description, model.RoleModel.Description);
            Assert.Equal(roles[0].Name, model.RoleModel.Name);
            Assert.Equal(definedClaims.Count, model.RoleModel.RoleClaims.Count);

            var claims = model.RoleModel.RoleClaims.Where(rc => rc.IsAssigned);
            Assert.Single(claims);
            Assert.Equal(expectedValue, claims.First().Claim);
        }

        [Fact(DisplayName = "Delete Role [Get] returns NotFound for null ID")]
        public async Task RoleManagement_Test21Async()
        {
            var model = new DeleteModel<ApplicationUser, ApplicationRole>(null, null, null);
            var result = await model.OnGetAsync(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Delete Role [Get] returns NotFound for DB not found")]
        public async Task RoleManagement_Test22Async()
        {
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
                );

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(null, repository, null);
            var result = await model.OnGetAsync(Guid.Empty.ToString());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Delete Role [Get] initializes RoleUser collection")]
        public async Task RoleManagement_Test23Async()
        {
            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };
            var roles = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var user = new ApplicationUser { Email = "MrTester@company.com", UserName = "MrTester@company.com", GivenName = "Chuck", Surname = "Fricke" };
            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            var userClaim = new IdentityUserClaim<string> { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = role.Name };
            var userClaims = new List<IdentityUserClaim<string>> { userClaim }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedGuids == GetDefinedGuids() &&
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == roles.Object &&
                db.Users == users.Object &&
                db.UserClaims == userClaims.Object
                );

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, null);
            var result = await model.OnGetAsync(role.Id);

            Assert.IsType<PageResult>(result);
            Assert.Equal(role.Id, model.RoleModel.Id);
            Assert.Single(model.RoleModel.RoleUsers);
            Assert.Equal(user.DisplayName, model.RoleModel.RoleUsers.First().Name);
            Assert.Equal(user.Email, model.RoleModel.RoleUsers.First().Email);
        }

        [Fact(DisplayName = "Delete Role [Post] returns NotFound for null ID")]
        public async Task RoleManagement_Test24Async()
        {
            var model = new DeleteModel<ApplicationUser, ApplicationRole>(null, null, null);
            var result = await model.OnPostAsync(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact(DisplayName = "Delete Role [Post] sends notification for DB not found")]
        public async Task RoleManagement_Test25Async()
        {
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole>().AsQueryable().BuildMockDbSet().Object
                );

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(null, repository, null)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(Guid.NewGuid().ToString());

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(1, model.TempData.Count);
            var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
            Assert.Contains(model.RoleModel.Name, notifications[0].Message);
        }

        [Fact(DisplayName = "Delete Role [Post] prevents delete of System Role")]
        public async Task RoleManagement_Test26Async()
        {
            var expectedMessage = "System Roles may not be deleted";

            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.SystemObject(null))
            );

#pragma warning disable CA2012 // Use ValueTasks correctly
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles.FindAsync(role.Id) == new ValueTask<ApplicationRole>(Task.FromResult(role)) &&
                db.UserClaims == new IdentityUserClaim<string>[] { }.AsQueryable().BuildMockDbSet().Object &&
                db.Users == new ApplicationUser[] { }.AsQueryable().BuildMockDbSet().Object
                );
#pragma warning restore CA2012 // Use ValueTasks correctly

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains(expectedMessage, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Error, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
            Assert.Contains(expectedMessage, logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "Delete Role [Post] handles DB exception")]
        public async Task RoleManagement_Test27Async()
        {
            var dbUpdateException =
                new DbUpdateException("One or more errors occurred. (An error occurred while updating the entries. See the inner exception for details.)",
                new DataException("The INSERT statement conflicted with the PRIMARY KEY constraint 'PK_ApplicationRole_Id'.")
                );

            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var user = new ApplicationUser("MrTester@company.com");
            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            var userClaim = new IdentityUserClaim<string> { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = role.Name };
            var userClaims = new List<IdentityUserClaim<string>> { userClaim }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.FindAsync(role.Id)).Returns(new ValueTask<ApplicationRole>(Task.FromResult(role)));
            repository.Setup(db => db.Roles.Remove(It.IsAny<ApplicationRole>())).Throws(dbUpdateException);
            repository.Setup(db => db.UserClaims).Returns(userClaims.Object);
            repository.Setup(db => db.Users).Returns(users.Object);

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains("Role", errors[0].ErrorMessage);
            Assert.Equal(dbUpdateException.GetBaseException().Message, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Error, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
            Assert.NotNull(logger.LogEntries[0].Exception);
        }

        [Fact(DisplayName = "Delete Role [Post] sends notification for delete")]
        public async Task RoleManagement_Test28Async()
        {
            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var dbsUserClaims = Array.Empty<IdentityUserClaim<string>>().AsQueryable().BuildMockDbSet().Object;
            var dbsUsers = Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object;

            var authManager = new Mock<IAuthorizationManager>();
            authManager
                .Setup(am => am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            authManager.Setup(am => am.RefreshRole(role.Id));

#pragma warning disable CA2012 // Use ValueTasks correctly
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles.FindAsync(role.Id) == new ValueTask<ApplicationRole>(Task.FromResult(role)) &&
                db.UserClaims == dbsUserClaims &&
                db.Users == dbsUsers &&
                db.Roles.Remove(It.IsAny<ApplicationRole>()) == null &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );
#pragma warning restore CA2012 // Use ValueTasks correctly

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager.Object, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(1, model.TempData.Count);
            var notifications = model.TempData.GetNotifications(model.TempData.Keys.First());
            Assert.Contains(role.Name, notifications[0].Message);
        }

        [Fact(DisplayName = "Delete Role [Post] logs success")]
        public async Task RoleManagement_Test29Async()
        {
            ApplicationRole deletedRole = null;

            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var dbsUserClaims = Array.Empty<IdentityUserClaim<string>>().AsQueryable().BuildMockDbSet().Object;
            var dbsUsers = Array.Empty<ApplicationUser>().AsQueryable().BuildMockDbSet().Object;

            var authManager = new Mock<IAuthorizationManager>();
            authManager
                .Setup(am => am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()))
                .Returns(Task.FromResult(AuthorizationResult.Success()));
            authManager.Setup(am => am.RefreshRole(role.Id));

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.FindAsync(role.Id)).Returns(new ValueTask<ApplicationRole>(Task.FromResult(role)));
            repository.Setup(db => db.UserClaims).Returns(dbsUserClaims);
            repository.Setup(db => db.Users).Returns(dbsUsers);
            repository.Setup(db => db.Roles.Remove(It.IsAny<ApplicationRole>()))
                .Callback((ApplicationRole ar) => { deletedRole = ar; })
                .Returns((EntityEntry<ApplicationRole>)null);
            repository.Setup(db => db.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager.Object, repository.Object, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Assert.IsType<RedirectToPageResult>(result);

            Assert.NotNull(deletedRole);
            Assert.True(ReferenceEquals(role, deletedRole));

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "Edit Role [Post] prevents update of System Role")]
        public async Task RoleManagement_Test30Async()
        {
            var expectedMessage = "You may not update the Claims assigned to a system Role";

            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            role.Claims.Add(new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = role.Name });

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.SystemObject(null))
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet().Object
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains(expectedMessage, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(nameof(ApplicationRole), logger.LogEntries[0].Message);
            Assert.Contains(role.Id, logger.LogEntries[0].Message);
            Assert.Contains(role.Name, logger.LogEntries[0].Message);
            Assert.Contains(expectedMessage, logger.LogEntries[0].Message);
        }

        [Fact(DisplayName = "Edit Role [Post] refreshes Role cache on success")]
        public async Task RoleManagement_Test31Async()
        {
            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            role.Claims.Add(new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = role.Name });
            var dbSet = new List<ApplicationRole> { role }.AsQueryable().BuildMockDbSet();

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var dbContext = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == dbSet.Object &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, dbContext, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            await model.OnPostAsync(string.Empty);

            Mock.Get(authManager).Verify(am => am.RefreshRole(role.Id));
        }

        [Fact(DisplayName = "Delete Role [Post] refreshes Role cache on success")]
        public async Task RoleManagement_Test32Async()
        {
            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var authManager = Mock.Of<IAuthorizationManager>(am =>
               am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

#pragma warning disable CA2012 // Use ValueTasks correctly
            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles.FindAsync(role.Id) == new ValueTask<ApplicationRole>(Task.FromResult(role)) &&
                db.UserClaims == new IdentityUserClaim<string>[0].AsQueryable().BuildMockDbSet().Object &&
                db.Users == new ApplicationUser[0].AsQueryable().BuildMockDbSet().Object &&
                db.Roles.Remove(role) == null &&
                db.SaveChangesAsync(default) == Task.FromResult(1)
                );
#pragma warning restore CA2012 // Use ValueTasks correctly

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Mock.Get(authManager).Verify(am => am.RefreshRole(role.Id));
        }

        [Fact(DisplayName = "Edit Role [Get] sets IsSystemRole")]
        public async Task RoleManagement_Test33Async()
        {
            var role = new ApplicationRole { Id = SysUiGuids.Role.UserManager, Name = nameof(SysUiGuids.Role.UserManager) };

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedGuids == GetDefinedGuids() &&
                am.DefinedClaims == GetDefinedClaims()
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, null);
            await model.OnGetAsync(role.Id);

            Assert.True(model.IsSystemRole);
        }

        [Fact(DisplayName = "Create Role [Post] handles failed elevation check")]
        public async Task RoleManagement_Test34Async()
        {
            var expectedMessage = "can not create a Role with more privileges";
            var expectedLogMessage = $"attempted to create {nameof(ApplicationRole)} with elevated privileges";

            var user = new ApplicationUser("TestUser@company.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var httpContext = Mock.Of<HttpContext>(hc =>
                hc.User == claimsPrincipal
                );

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Elevation(null))
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>();

            var logger = new TestLogger<CreateModel>();

            var model = new CreateModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = new RoleModel { Name = "TestRole", Description = "Can do tester stuff." },
                PageContext = new PageContext { HttpContext = httpContext }
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains(expectedMessage, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(user.Id, logger.LogEntries[0].Message);
            Assert.Contains(user.UserName, logger.LogEntries[0].Message);
            Assert.Contains(expectedLogMessage, logger.LogEntries[0].Message);
            Assert.Null(logger.LogEntries[0].Exception);
        }

        [Fact(DisplayName = "Edit Role [Post] handles failed elevation check")]
        public async Task RoleManagement_Test35Async()
        {
            var expectedMessage = "can not give a Role more privileges";
            var expectedLogMessage = $"attempted to give {nameof(ApplicationRole)} elevated privileges";

            var role = new ApplicationRole { Name = "TestRole", Description = "Can do tester stuff." };
            role.Claims.Add(new IdentityRoleClaim<string> { Id = 1, RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = role.Name });

            var user = new ApplicationUser("TestUser@StEmilian.com");
            var claimsPrincipal = Mock.Of<ClaimsPrincipal>(cp =>
                cp.Claims == new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) } &&
                cp.Identity.Name == user.UserName
                );

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Elevation(null))
                );

            var httpContext = Mock.Of<HttpContext>(hc =>
                hc.User == claimsPrincipal
                );

            var repository = Mock.Of<IRepository<ApplicationUser, ApplicationRole>>(db =>
                db.Roles == new[] { role }.AsQueryable().BuildMockDbSet().Object
                );

            var logger = new TestLogger<EditModel>();

            var model = new EditModel<ApplicationUser, ApplicationRole>(authManager, repository, logger)
            {
                RoleModel = CreateModelFromRole(role),
                PageContext = new PageContext { HttpContext = httpContext },
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(string.Empty);

            Assert.IsType<PageResult>(result);

            Assert.False(model.ModelState.IsValid);
            Assert.Equal(2, model.ModelState.ErrorCount);
            var errors = model.ModelState[string.Empty].Errors;
            Assert.Contains(expectedMessage, errors[1].ErrorMessage);

            Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Information, logger.LogEntries[0].LogLevel);
            Assert.Contains(user.Id, logger.LogEntries[0].Message);
            Assert.Contains(user.UserName, logger.LogEntries[0].Message);
            Assert.Contains(expectedLogMessage, logger.LogEntries[0].Message);
            Assert.Null(logger.LogEntries[0].Exception);
        }

        [Fact(DisplayName = "Delete Role [Post] removes UserClaim associated with Role")]
        public async Task RoleManagement_Test36Async()
        {
            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var user = new ApplicationUser("MrTester@company.com");
            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            var userClaim = new IdentityUserClaim<string> { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = role.Name };
            var userClaims = new List<IdentityUserClaim<string>> { userClaim }.AsQueryable().BuildMockDbSet();

            IdentityUserClaim<string>[] removedClaims = null;

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.FindAsync(role.Id)).Returns(new ValueTask<ApplicationRole>(Task.FromResult(role)));
            repository.Setup(db => db.Roles.Remove(It.IsAny<ApplicationRole>())).Returns((EntityEntry<ApplicationRole>)null);
            repository.Setup(db => db.UserClaims).Returns(userClaims.Object);
            repository.Setup(db => db.Users).Returns(users.Object);
            repository.Setup(db => db.UserClaims.RemoveRange(It.IsAny<IdentityUserClaim<string>[]>()))
                .Callback((IdentityUserClaim<string>[] claims) => removedClaims = claims);

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Assert.NotNull(removedClaims);
            Assert.Single(removedClaims);
            Assert.Equal(userClaim, removedClaims[0]);
        }

        [Fact(DisplayName = "Delete Role [Post] refreshes UserClaim cache on success")]
        public async Task RoleManagement_Test37Async()
        {
            var role = new ApplicationRole { Name = "TestManager", Description = "Does all things that are testing." };

            var user = new ApplicationUser("MrTester@company.com");
            var users = new List<ApplicationUser> { user }.AsQueryable().BuildMockDbSet();

            var userClaim = new IdentityUserClaim<string> { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = role.Name };
            var userClaims = new List<IdentityUserClaim<string>> { userClaim }.AsQueryable().BuildMockDbSet();

            IdentityUserClaim<string>[] removedClaims = null;

            var authManager = Mock.Of<IAuthorizationManager>(am =>
                am.DefinedClaims == GetDefinedClaims() &&
                am.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<ApplicationRole>(), It.IsAny<AppClaimRequirement>()) == Task.FromResult(AuthorizationResult.Success())
                );

            var repository = new Mock<IRepository<ApplicationUser, ApplicationRole>>();
            repository.Setup(db => db.Roles.FindAsync(role.Id)).Returns(new ValueTask<ApplicationRole>(Task.FromResult(role)));
            repository.Setup(db => db.Roles.Remove(It.IsAny<ApplicationRole>())).Returns((EntityEntry<ApplicationRole>)null);
            repository.Setup(db => db.UserClaims).Returns(userClaims.Object);
            repository.Setup(db => db.Users).Returns(users.Object);
            repository.Setup(db => db.UserClaims.RemoveRange(It.IsAny<IdentityUserClaim<string>[]>()))
                .Callback((IdentityUserClaim<string>[] claims) => removedClaims = claims);

            var logger = new TestLogger<DeleteModel>();

            var model = new DeleteModel<ApplicationUser, ApplicationRole>(authManager, repository.Object, logger)
            {
                RoleModel = CreateModelFromRole(role),
                TempData = new TestTempDataDictionary()
            };

            var result = await model.OnPostAsync(role.Id);

            Mock.Get(authManager).Verify(am => am.RefreshUser(user.Id));
        }
    }
}
