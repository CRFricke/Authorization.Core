using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRFricke.Authorization.Core.UI.Data
{
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
    public class AuthUiContext<TUser, TRole> : AuthDbContext<TUser, TRole>
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
        public override void SeedDatabase()
        {
            base.SeedDatabase();

            var normalizer = new UpperInvariantLookupNormalizer();

            var role = Roles.Find(SysGuids.Role.Administrator);
            if (role != null)
            {
                if (role.Description == null)
                {
                    role.Description = "Administrators have access to all portions of the application.";

                    Roles.Update(role);
                }
            }

            role = Roles.Find(SysUiGuids.Role.RoleManager);
            if (role == null)
            {
                role = new TRole
                {
                    Id = SysUiGuids.Role.RoleManager,
                    Name = nameof(SysUiGuids.Role.RoleManager),
                    Description = "RoleManagers are responsible for managing the application's Roles.",
                    NormalizedName = normalizer.NormalizeName(nameof(SysUiGuids.Role.RoleManager))
                };
                ((AuthUiRole)role).SetClaims(SysClaims.Role.DefinedClaims);

                Roles.Add(role);
            }

            role = Roles.Find(SysUiGuids.Role.UserManager);
            if (role == null)
            {
                role = new TRole
                {
                    Id = SysUiGuids.Role.UserManager,
                    Name = nameof(SysUiGuids.Role.UserManager),
                    Description = "UserManagers are responsible for managing the application's Users.",
                    NormalizedName = normalizer.NormalizeName(nameof(SysUiGuids.Role.UserManager))
                };
                ((AuthUiRole)role).SetClaims(SysClaims.User.DefinedClaims);

                Roles.Add(role);
            }

            SaveChanges();
        }
    }
}
