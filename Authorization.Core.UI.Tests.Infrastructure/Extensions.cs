using Authorization.Core.UI.Test.Web.Data;
using CRFricke.Authorization.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Authorization.Core.UI.Tests.Infrastructure;

public static class Extensions
{
    extension(WebAppFactory webAppFixture)
    {
        /// <summary>
        /// Deletes the specified <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="roleId">
        /// The Id (database key) of the role to be deleted.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> to be used to <see langword="await"/> on.
        /// </returns>
        public async Task DeleteRoleAsync(string roleId)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            var dbRole = await dbContext.Roles.FindAsync(roleId);
            if (dbRole is not null)
            {
                dbContext.Roles.Remove(dbRole);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Deletes the specified <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="userId">
        /// The Id (database key) of the user to be deleted.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> to be used to <see langword="await"/> on.
        /// </returns>
        public async Task DeleteUserAsync(string userId)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            var dbUser = await dbContext.Users.FindAsync(userId);
            if (dbUser is not null)
            {
                dbContext.Users.Remove(dbUser);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Ensures that the specified application role exists in the database.
        /// </summary>
        /// <param name="role">The application role to be verified.</param>
        /// <returns>
        /// The <see cref="Task{ApplicationRole}"/> to be used to <see langword="await"/> on.
        /// </returns>
        public async Task<ApplicationRole> EnsureRoleAsync(ApplicationRole role)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            var dbRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
            if (dbRole is not null)
            {
                return dbRole;
            }

            var normalizer = webAppFixture.Services.GetRequiredService<ILookupNormalizer>();
            role.NormalizedName = normalizer.NormalizeName(role.Name);

            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();

            return role;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
        /// </summary>
        /// <param name="login">
        /// A <see cref="Login"/> object that contains the user's email address and password.
        /// <em>Note:</em> the password is only used if the user does not exist and needs to be created.
        /// </param>
        /// <param name="userClaimValue">
        /// An optional claim value that specifies an application role to be assigned to the user.
        /// </param>
        /// <returns>The specified <see cref="ApplicationUser"/>.</returns>
        public ApplicationUser EnsureUser(
            Login login,
            string? userClaimValue = null)
        {
            return EnsureUserAsync(
                webAppFixture,
                new() { Email = login.Email },
                login.Password,
                userClaimValue).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
        /// </summary>
        /// <param name="login">
        /// A <see cref="Login"/> object that contains the user's email address and password.
        /// <em>Note:</em> the password is only used if the user does not exist and needs to be created.
        /// </param>
        /// <param name="userClaimValue">
        /// An optional claim value that specifies an <see cref="ApplicationRole"/> to be assigned to the user.
        /// </param>
        /// <returns>The <see cref="Task{ApplicationUser}"/> to be used to <see langword="await"/> on.</returns>
        public async Task<ApplicationUser> EnsureUserAsync(
            Login login,
            string? userClaimValue = null)
        {
            return await EnsureUserAsync(
                webAppFixture,
                new() { Email = login.Email },
                login.Password,
                userClaimValue);
        }

        /// <summary>
        /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
        /// </summary>
        /// <param name="user">The <see cref="ApplicationUser"/> object to be verified.</param>
        /// <param name="password">
        /// The password for the user. <em>Note:</em> this parameter is only used if the 
        /// <see cref="ApplicationUser"/> does not exist and needs to be created.
        /// </param>
        /// <param name="userClaimValue">
        /// An optional claim value that specifies an <see cref="ApplicationRole"/> to be assigned to the user.
        /// </param>
        /// <returns>The <see cref="Task{ApplicationUser}"/> to be used to <see langword="await"/> on.</returns>
        public async Task<ApplicationUser> EnsureUserAsync(
            ApplicationUser user,
            string password,
            string? userClaimValue = null)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (dbUser is not null)
            {
                return dbUser;
            }

            var hasher = webAppFixture.Services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            var normalizer = webAppFixture.Services.GetRequiredService<ILookupNormalizer>();

            user.EmailConfirmed = true;
            user.NormalizedEmail = normalizer.NormalizeEmail(user.Email);
            user.NormalizedUserName = user.NormalizedEmail;
            user.PasswordHash = hasher.HashPassword(user, password);
            user.UserName = user.Email;

            await dbContext.Users.AddAsync(user);

            bool userNeedsRefresh = false;

            if (userClaimValue is not null)
            {
                var userClaim = await dbContext.UserClaims.FirstOrDefaultAsync(uc =>
                    uc.UserId == user.Id &&
                    uc.ClaimType == ClaimTypes.Role &&
                    uc.ClaimValue == userClaimValue
                    );

                if (userClaim is null)
                {
                    userClaim = new()
                    {
                        UserId = user.Id,
                        ClaimType = ClaimTypes.Role,
                        ClaimValue = userClaimValue
                    };

                    await dbContext.UserClaims.AddAsync(userClaim);
                    userNeedsRefresh = true;
                }
            }

            await dbContext.SaveChangesAsync();

            if (userNeedsRefresh)
            {
                var authManager = webAppFixture.Services.GetRequiredService<IAuthorizationManager>();
                authManager.RefreshUser(user.Id);
            }

            return user;
        }

        /// <summary>
        /// Retrieves the specified <see cref="ApplicationRole"/> from the database. 
        /// Returns <see langword="null"/> if there is no role with the specified key.
        /// </summary>
        /// <param name="roleId">The Id (database key) of the role to be retrieved.</param>
        /// <returns>The <see cref="Task{ApplicationRole?}"/> to be used to <see langword="await"/> on.</returns>
        public async Task<ApplicationRole?> GetRoleByIdAsync(string roleId)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            return await dbContext.Roles
                .Where(r => r.Id == roleId)
                .Include(r => r.Claims)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves the specified <see cref="ApplicationRole"/> from the database.
        /// Returns <see langword="null"/> if there is no role with the specified name.
        /// </summary>
        /// <param name="roleName">The name of the role to be retrieved.</param>
        /// <returns>The <see cref="Task{ApplicationRole?}"/> to be used to <see langword="await"/> on.</returns>
        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            return await dbContext.Roles
                .Where(r => r.Name == roleName)
                .Include(r => r.Claims)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves the collection of application roles from the database.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="DbSet{ApplicationRole}"/> can be used to query, add, update, or remove roles. 
        /// Changes made to the set will be tracked by the underlying database context and persisted when saved.
        /// </remarks>
        /// <returns>
        /// A <see cref="DbSet{ApplicationRole}"/> containing all roles defined in the application. 
        /// The set may be empty if no roles exist.
        /// </returns>
        public DbSet<ApplicationRole> GetRoles()
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            return dbContext.Roles;
        }

        /// <summary>
        /// Asynchronously retrieves a user by email address, including associated claims.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="ApplicationUser"/> is loaded with claims and is not tracked by the context. 
        /// This method is suitable for read-only scenarios.
        /// </remarks>
        /// <param name="userEmail">
        /// The email address of the user to retrieve. Cannot be null or empty.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ApplicationUser?}"/> that represents the asynchronous operation. 
        /// The task result contains the user matching the specified email address, or null if no user is found.
        /// </returns>
        public async Task<ApplicationUser?> GetUserByEmailAsync(string userEmail)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            return await dbContext.Users
                .Where(u => u.Email == userEmail)
                .Include(u => u.Claims)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">
        /// The unique identifier of the user to retrieve. Cannot be null or empty.
        /// </param>
        /// <returns>
        /// A <see cref="Task{ApplicationUser?}"/> that represents the asynchronous operation. 
        /// The task result contains the user associated with the specified identifier, or null if no user is found.
        /// </returns>
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            return await dbContext.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Claims)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Verifies that the specified <see cref="ApplicationRole"/> exists in the database.
        /// </summary>
        /// <param name="roleName">The name of the <see cref="ApplicationRole"/> to be verified.</param>
        /// <returns>The <see cref="Task"/> to be used to <see langword="await"/> on.</returns>
        /// <exception cref="Xunit.Sdk.XunitException">
        /// Thrown if the role does not exist.
        /// </exception>
        public async Task VerifyRoleExistsAsync(string roleName)
        {
            var dbContext = webAppFixture.Services.GetRequiredService<ApplicationDbContext>();
            Assert.True(await dbContext.Roles.AnyAsync(r => r.Name == roleName));
        }
    }
}
