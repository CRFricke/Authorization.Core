using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace CRFricke.Authorization.Core.Data
{
    /// <summary>
    /// Base class for a database context used by the Authorization.Core library.
    /// </summary>
    public class AuthDbContext : AuthDbContext<AuthUser, AuthRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using default options.
        /// </summary>
        protected AuthDbContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AuthDbContext"/> instance.</param>
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AuthDbContext"/> instance.</param>
        protected AuthDbContext(DbContextOptions options) : base(options)
        { }
    }

    /// <summary>
    /// Base class for a database context used by the Authorization.Core library.
    /// </summary>
    /// <typeparam name="TUser">The <see cref="Type"/> of user objects.</typeparam>
    /// <typeparam name="TRole">The <see cref="Type"/> of role objects.</typeparam>
    public abstract class AuthDbContext<TUser, TRole> :
        IdentityDbContext<TUser, TRole, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>,
        IRepository<TUser, TRole>
        where TUser : AuthUser, new()
        where TRole : AuthRole, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using default options.
        /// </summary>
        protected AuthDbContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AuthDbContext"/> instance.</param>
        public AuthDbContext(DbContextOptions<AuthDbContext<TUser, TRole>> options) : base(options)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="AuthDbContext"/> instance.</param>
        protected AuthDbContext(DbContextOptions options) : base(options)
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

            builder.Entity<TRole>()
                .HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TUser>()
                .HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }

        /// <summary>
        /// Called to seed the database.
        /// </summary>
        public virtual void SeedDatabase()
        {
            var normalizer = new UpperInvariantLookupNormalizer();

            var role = Roles.Find(SysGuids.Role.Administrator);
            if (role == null)
            {
                role = new TRole
                {
                    Id = SysGuids.Role.Administrator,
                    Name = nameof(SysGuids.Role.Administrator),
                    NormalizedName = normalizer.NormalizeName(nameof(SysGuids.Role.Administrator))
                };

                Roles.Add(role);
            }

            var user = Users.Find(SysGuids.User.Administrator);
            if (user == null)
            {
                var email = "Admin@company.com";

                user = new TUser
                {
                    Id = SysGuids.User.Administrator,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEPPGh+zIZ8PSo5IQ1IjPnVqUph0c0utc5Kd37NmA8U1Fhe+MEu3gbxP81sPcxkJaMQ==", // "Administrat0r!"
                    UserName = email
                };
                ((AuthUser)user).SetClaims(role.Name);

                Users.Add(user);
            }

            SaveChanges();
        }
    }
}
