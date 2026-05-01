using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace CRFricke.Authorization.Core.UI.Data;

/// <summary>
/// Base class for a database context used by the Authorization.Core library.
/// </summary>
public class AuthUiContext : AuthDbContext<AuthUiUser, AuthUiRole>
{
    /// <summary>
    /// Creates a new instance of the <see cref="AuthUiContext"/> class using default options.
    /// </summary>
    public AuthUiContext()
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="AuthUiContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the new <see cref="AuthUiContext"/> instance.</param>
    public AuthUiContext(DbContextOptions<AuthUiContext> options) : base(options)
    { }

    /// <summary>
    /// Creates a new instance of the <see cref="AuthUiContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the new <see cref="AuthUiContext"/> instance.</param>
    protected AuthUiContext(DbContextOptions options) : base(options)
    { }
}

/// <summary>
/// Base class for a database context used by the Authorization.Core library.
/// </summary>
/// <typeparam name="TUser">The <see cref="Type"/> of user objects.</typeparam>
/// <typeparam name="TRole">The <see cref="Type"/> of role objects.</typeparam>
public class AuthUiContext<
[DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
[DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : AuthDbContext<TUser, TRole>
    where TUser: AuthUiUser, new()
    where TRole: AuthUiRole, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthUiContext"/> class using default options.
    /// </summary>
    protected AuthUiContext()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthUiContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the new <see cref="AuthUiContext"/> instance.</param>
    public AuthUiContext(DbContextOptions<AuthUiContext<TUser, TRole>> options) : base(options)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthUiContext"/> class using the specified options.
    /// </summary>
    /// <param name="options">The options to be used by the new <see cref="AuthUiContext"/> instance.</param>
    protected AuthUiContext(DbContextOptions options) : base(options)
    { }

    /// <summary>
    /// Configures the schema needed for the Authorization.Core library.
    /// </summary>
    /// <param name="builder">
    /// The builder being used to construct the model for this context.
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    /// <inheritdoc/>
    public override async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
    {
        await base.SeedDatabaseAsync(serviceProvider).ConfigureAwait(false);

        var normalizer = serviceProvider.GetRequiredService<ILookupNormalizer>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AuthUiContext>();

        var role = await Roles.FindAsync(SysGuids.Role.Administrator).ConfigureAwait(false);
        if (role != null)
        {
            if (role.Description == null)
            {
                role.Description = "Administrators have access to all portions of the application.";

                Roles.Update(role);
                logger.LogRoleSeeded(
                    typeof(TRole).Name,
                    role.Name,
                    role.Id
                    );
            }
        }

        role = await Roles.FindAsync(SysUiGuids.Role.RoleManager).ConfigureAwait(false);
        if (role == null)
        {
            role = new TRole
            {
                Id = SysUiGuids.Role.RoleManager,
                Name = nameof(SysUiGuids.Role.RoleManager),
                Description = "RoleManagers are responsible for managing the application's Roles.",
                NormalizedName = normalizer.NormalizeName(nameof(SysUiGuids.Role.RoleManager))
            };
            role.SetClaims<AuthUiRole>(SysClaims.Role.DefinedClaims);

            await Roles.AddAsync(role).ConfigureAwait(false);
            logger.LogRoleCreatedDuringSeed(
                typeof(TRole).Name,
                role.Name,
                role.Id
                );
        }

        role = await Roles.FindAsync(SysUiGuids.Role.UserManager).ConfigureAwait(false);
        if (role == null)
        {
            role = new TRole
            {
                Id = SysUiGuids.Role.UserManager,
                Name = nameof(SysUiGuids.Role.UserManager),
                Description = "UserManagers are responsible for managing the application's Users.",
                NormalizedName = normalizer.NormalizeName(nameof(SysUiGuids.Role.UserManager))
            };
            role.SetClaims<AuthUiRole>(SysClaims.User.DefinedClaims);

            await Roles.AddAsync(role).ConfigureAwait(false);
            logger.LogRoleCreatedDuringSeed(
                typeof(TRole).Name,
                role.Name,
                role.Id
                );
        }

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            await SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogSaveChangesFailed(ex);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
