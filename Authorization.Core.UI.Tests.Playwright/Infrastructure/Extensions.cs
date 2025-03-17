using Authorization.Core.UI.Test.Web.Data;
using CRFricke.Authorization.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Web;

namespace Authorization.Core.UI.Tests.Playwright.Infrastructure;

internal static class Extensions
{
    /// <summary>
    /// The default base <see cref="Uri"/> of the web application under test.
    /// </summary>
    internal static string HostUri { get; } = "https://localhost/";

    /// <summary>
    /// Performs the configuration required to intercept Playwright HTTP requests and 
    /// forward them to the WebApplicationFactory used for testing the application.
    /// </summary>
    /// <param name="context">The Playwright <see cref="IBrowserContext"/> to be configured.</param>
    /// <param name="httpClient">
    /// The <see cref="HttpClient"/> the Playwright HTTP requests are to be forwarded to.
    /// </param>
    /// <param name="route">
    /// An optional route that defines the destination of the Playwright HTTP requests to be 
    /// intercepted. If not specified, defaults to "https://localhost/**".
    /// </param>
    /// <returns>The <see cref="TaskAwaiter"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task ConfigureRouteAsync(
        this IBrowserContext context, 
        HttpClient httpClient, 
        string? route = null )
    {
        route ??= $"{HostUri}**";

        await context.RouteAsync(route, async route =>
        {
            var request = route.Request;
            var content = request.PostDataBuffer is { } postDataBuffer
                ? new ByteArrayContent(postDataBuffer) : null;

            var requestMessage = new HttpRequestMessage(new(request.Method), request.Url)
            {
                Content = content
            };

            foreach (var header in request.Headers)
            {
                try
                {
                    if (header.Key.StartsWith("content-"))
                    {
                        requestMessage.Content!.Headers.Add(header.Key, header.Value);
                    }
                    else
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
                catch (Exception ex)
                {
                    TestContext.Out.WriteLine("Error copying HTTP request headers in Playwright.BrowserContext Route handler.");
                    TestContext.Out.WriteLine(ex.ToString());
                }
            }

            var response = await httpClient.SendAsync(requestMessage);
            var responseBody = await response.Content.ReadAsByteArrayAsync();
            var responseHeaders = response.Content.Headers
                .Select(h => KeyValuePair.Create(h.Key, h.Value.First()));

            await route.FulfillAsync(new()
            {
                BodyBytes = responseBody,
                Headers = responseHeaders,
                Status = (int)response.StatusCode
            });
        });
    }

    /// <summary>
    /// Uses Playwright to log the specified <see cref="ApplicationUser"/> into the web application.
    /// </summary>
    /// <param name="page">The Playwright <see cref="IPage"/> object to be used to log in.</param>
    /// <param name="login">A <see cref="Login"/> object that contains the user's email address and password.</param>
    /// <param name="returnUrl">An optional URL to return to after the user is logged in.</param>
    /// <returns>The <see cref="TaskAwaiter"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task LogUserInAsync(
        this IPage page, 
        Login login, 
        string? returnUrl = null )
    {
        var url = $"{HostUri}Identity/Account/Login";
        if (returnUrl != null)
        {
            url += "?ReturnUrl=" + HttpUtility.UrlEncode(returnUrl);
        }

        await page.GotoAsync(url);
        await page.GetByPlaceholder("name@example.com").FillAsync(login.Email);
        await page.GetByPlaceholder("password").FillAsync(login.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }

    /// <summary>
    /// Deletes the specified <see cref="ApplicationRole"/>.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="roleId">The Id (database key) of the role to be deleted.</param>
    /// <returns>The <see cref="TaskAwaiter"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task DeleteRoleAsync(
        this WebAppFactory webAppFactory, 
        string roleId )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
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
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="userId">The Id (database key) of the user to be deleted.</param>
    /// <returns>The <see cref="TaskAwaiter"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task DeleteUserAsync(
        this WebAppFactory webAppFactory, 
        string userId )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
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
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="role">The application role to be verified.</param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationRole}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationRole> EnsureRoleAsync(
        this WebAppFactory webAppFactory, 
        ApplicationRole role )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        var dbRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
        if (dbRole is not null)
        {
            return dbRole;
        }

        var normalizer = webAppFactory.Services.GetRequiredService<ILookupNormalizer>();
        role.NormalizedName = normalizer.NormalizeName(role.Name);

        await dbContext.Roles.AddAsync(role);
        await dbContext.SaveChangesAsync();

        return role;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="login">
    /// A <see cref="Login"/> object that contains the user's email address and password.
    /// <em>Note:</em> the password is only used if the user does not exist and needs to be created.
    /// </param>
    /// <param name="userClaimValue">
    /// An optional claim value that specifies an application role to be assigned to the user.
    /// </param>
    /// <returns>The specified <see cref="ApplicationUser"/>.</returns>
    internal static ApplicationUser EnsureUser(
        this WebAppFactory webAppFactory,
        Login login,
        string? userClaimValue = null )
    {
        return EnsureUserAsync(
            webAppFactory, 
            new() { Email = login.Email }, 
            login.Password, 
            userClaimValue).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="login">
    /// A <see cref="Login"/> object that contains the user's email address and password.
    /// <em>Note:</em> the password is only used if the user does not exist and needs to be created.
    /// </param>
    /// <param name="userClaimValue">
    /// An optional claim value that specifies an <see cref="ApplicationRole"/> to be assigned to the user.
    /// </param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationUser}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationUser> EnsureUserAsync(
        this WebAppFactory webAppFactory, 
        Login login, 
        string? userClaimValue = null )
    {
        return await EnsureUserAsync(
            webAppFactory, 
            new() { Email = login.Email }, 
            login.Password, 
            userClaimValue);
    }

    /// <summary>
    /// Ensures that the specified <see cref="ApplicationUser"/> exists in the database.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="user">The <see cref="ApplicationUser"/> object to be verified.</param>
    /// <param name="password">
    /// The password for the user. <em>Note:</em> this parameter is only used if the 
    /// <see cref="ApplicationUser"/> does not exist and needs to be created.
    /// </param>
    /// <param name="userClaimValue">
    /// An optional claim value that specifies an <see cref="ApplicationRole"/> to be assigned to the user.
    /// </param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationUser}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationUser> EnsureUserAsync(
        this WebAppFactory webAppFactory, 
        ApplicationUser user, 
        string password, 
        string? userClaimValue = null )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (dbUser is not null)
        {
            return dbUser;
        }

        var hasher = webAppFactory.Services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        var normalizer = webAppFactory.Services.GetRequiredService<ILookupNormalizer>();

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
            var authManager = webAppFactory.Services.GetRequiredService<IAuthorizationManager>();
            authManager.RefreshUser(user.Id);
        }

        return user;
    }

    /// <summary>
    /// Retrieves the specified <see cref="ApplicationRole"/> from the database. 
    /// Returns <see langword="null"/> if there is no role with the specified key.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="roleId">The Id (database key) of the role to be retrieved.</param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationRole?}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationRole?> GetRoleByIdAsync(
        this WebAppFactory webAppFactory, 
        string roleId )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
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
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="roleName">The name of the role to be retrieved.</param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationRole?}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationRole?> GetRoleByNameAsync(
        this WebAppFactory webAppFactory, 
        string roleName )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Roles
            .Where(r => r.Name == roleName)
            .Include(r => r.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Returns the ApplicationRoles contained in the database.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <returns>A <see cref="DbSet{TEntity}"/> of the <see cref="ApplicationRole">ApplicationRoles</see> contained in the database.</returns>
    internal static DbSet<ApplicationRole> GetRoles(this WebAppFactory webAppFactory)
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        return dbContext.Roles;
    }

    /// <summary>
    /// Retrieves the specified <see cref="ApplicationUser"/> from the database.  
    /// Returns <see langword="null"/> if there is no user with the specified email.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="userEmail">The email address of the user to be retrieved.</param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationUser?}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationUser?> GetUserByEmailAsync(
        this WebAppFactory webAppFactory, 
        string userEmail )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users
            .Where(u => u.Email == userEmail)
            .Include(u => u.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves the specified <see cref="ApplicationUser"/> from the database. 
    /// Returns <see langword="null"/> if there is no user with the specified key.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="userId">The Id (database key) of the user to be retrieved.</param>
    /// <returns>The <see cref="TaskAwaiter{ApplicationUser?}"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task<ApplicationUser?> GetUserByIdAsync(
        this WebAppFactory webAppFactory, 
        string userId )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        return await dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Claims)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    /// <summary>
    /// Verifies that the specified <see cref="ApplicationRole"/> exists in the database.
    /// Throws an NUnit exception if the role does not exist.
    /// </summary>
    /// <param name="webAppFactory">
    /// The <see cref="WebAppFactory"/> whose services are to be used to access the database.
    /// </param>
    /// <param name="roleName">The name of the <see cref="ApplicationRole"/> to be verified.</param>
    /// <returns>The <see cref="TaskAwaiter"/> to be used to <see langword="await"/> on.</returns>
    internal static async Task VerifyRoleExistsAsync(
        this WebAppFactory webAppFactory, 
        string roleName )
    {
        var dbContext = webAppFactory.Services.GetRequiredService<ApplicationDbContext>();
        Assert.That(await dbContext.Roles.AnyAsync(r => r.Name == roleName), Is.True);
    }
}
