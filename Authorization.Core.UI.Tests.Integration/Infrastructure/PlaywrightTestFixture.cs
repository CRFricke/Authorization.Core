using Authorization.Core.UI.Test.Web.Data;
using Authorization.Core.UI.Tests.Integration.Infrastructure.Playwright;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure;

public class PlaywrightTestFixture : BrowserFixture, IAsyncLifetime
{
    #region Logins

    public record Login(string Email, string Password);

    internal static class Logins
    {
        public static Login Administrator => new("admin@company.com", "Administrat0r!");

        public static Login RoleManager => new("role.guy@company.com", "R0le.Guy!");

        public static Login UserManager => new("user.guy@company.com", "U5er.Guy!");
    }

    #endregion

    internal readonly WebAppFactory WebAppFactory = new();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await EnsureUserAsync(Logins.RoleManager, nameof(SysUiGuids.Role.RoleManager));
        await EnsureUserAsync(Logins.UserManager, nameof(SysUiGuids.Role.UserManager));
    }

    public async Task EnsureUserAsync(Login login, string? userClaimValue = null)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
        if (user is null)
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
            var normalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();

            user = new()
            {
                Email = login.Email,
                NormalizedEmail = normalizer.NormalizeEmail(login.Email),
                EmailConfirmed = true,
                UserName = login.Email,
                NormalizedUserName = normalizer.NormalizeName(login.Email),
                PasswordHash = hasher.HashPassword(user!, login.Password)
            };

            await dbContext.Users.AddAsync(user);
        }

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
            var authManager = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();
            authManager.RefreshUser(user.Id);
        }
    }

    public async Task<ApplicationUser> EnsureUserAsync(ApplicationUser user, string password, string? userClaimValue = null)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (dbUser is not null)
        {
            return dbUser;
        }

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        var normalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();

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
            var authManager = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();
            authManager.RefreshUser(user.Id);
        }

        return user;
    }

    public async Task EnsureRoleAsync(ApplicationRole role)
    {
        using var scope = WebAppFactory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (! await dbContext.Roles.AnyAsync(r => r.Id == role.Id))
        {
            var normalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();
            role.NormalizedName = normalizer.NormalizeName(role.Name);

            await dbContext.Roles.AddAsync(role);
            await dbContext.SaveChangesAsync();
        }
    }
}
