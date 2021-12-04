using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRFricke.Authorization.Core.UI.Data
{
    /// <summary>
    /// Base class for a database context used by the Authorization.Core library.
    /// </summary>
    public class AppDbContext : AuthDbContext<AppUser, AppRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class using default options.
        /// </summary>
        protected AppDbContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AppDbContext"/> instance.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
    }

    /// <summary>
    /// Base class for a database context used by the Authorization.Core library.
    /// </summary>
    /// <typeparam name="TUser">The <see cref="Type"/> of user objects.</typeparam>
    /// <typeparam name="TRole">The <see cref="Type"/> of role objects.</typeparam>
    public class AppDbContext<TUser, TRole> : AuthDbContext<TUser, TRole>
        where TUser: AppUser, new()
        where TRole: AppRole, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppDbContext"/> class using default options.
        /// </summary>
        protected AppDbContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AuthDbContext"/> instance.</param>
        public AppDbContext(DbContextOptions options) : base(options)
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

            var normalizer = new UpperInvariantLookupNormalizer();

            builder.Entity<TRole>().HasData(
                new TRole
                {
                    Id = SysGuids.Role.Administrator,
                    Name = nameof(SysGuids.Role.Administrator),
                    Description = "Administrators have access to all portions of the application.",
                    NormalizedName = normalizer.NormalizeName(nameof(SysGuids.Role.Administrator))
                },
                new TRole
                {
                    Id = SysGuids.Role.RoleManager,
                    Name = nameof(SysGuids.Role.RoleManager),
                    Description = "RoleManagers are responsible for managing the application's Roles.",
                    NormalizedName = normalizer.NormalizeName(nameof(SysGuids.Role.RoleManager))
                },
                new TRole
                {
                    Id = SysGuids.Role.UserManager,
                    Name = nameof(SysGuids.Role.UserManager),
                    Description = "UserManagers are responsible for managing the application's Users.",
                    NormalizedName = normalizer.NormalizeName(nameof(SysGuids.Role.UserManager))
                }); ;

            var id = 1;

            builder.Entity<IdentityRoleClaim<string>>().HasData(
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.Create,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.Delete,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.List,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.Read,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.Update,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.Role.UpdateClaims,
                    Id = id++,
                    RoleId = SysGuids.Role.RoleManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.Create,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.Delete,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.List,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.Read,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.Update,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                },
                new IdentityRoleClaim<string>
                {
                    ClaimType = SysClaims.ClaimType,
                    ClaimValue = SysClaims.User.UpdateClaims,
                    Id = id++,
                    RoleId = SysGuids.Role.UserManager
                });

            var email = "Admin@company.com";

            builder.Entity<TUser>().HasData(
                new TUser
                {
                    Id = SysGuids.User.Administrator,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEPPGh+zIZ8PSo5IQ1IjPnVqUph0c0utc5Kd37NmA8U1Fhe+MEu3gbxP81sPcxkJaMQ==", // "Administrat0r!"
                    UserName = email
                });

            id = 1;

            builder.Entity<IdentityUserClaim<string>>().HasData(
                new IdentityUserClaim<string>
                {
                    ClaimType = ClaimTypes.Role,
                    ClaimValue = nameof(SysGuids.Role.Administrator),
                    Id = id++,
                    UserId = SysGuids.User.Administrator
                });
        }
    }
}
