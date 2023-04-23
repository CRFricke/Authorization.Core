using CRFricke.EF.Core.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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
    public abstract class AuthDbContext<
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole > 
        : IdentityDbContext<TUser, TRole, string>, IRepository<TUser, TRole>, ISeedingContext
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

        /// <inheritdoc/>
        public virtual async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            var hasher = serviceProvider.GetRequiredService<IPasswordHasher<TUser>>();
            var normalizer = serviceProvider.GetRequiredService<ILookupNormalizer>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<AuthDbContext>();

            var role = await Roles.FindAsync(SysGuids.Role.Administrator);
            if (role == null)
            {
                role = new TRole
                {
                    Id = SysGuids.Role.Administrator,
                    Name = nameof(SysGuids.Role.Administrator),
                    NormalizedName = normalizer.NormalizeName(nameof(SysGuids.Role.Administrator))
                };

                await Roles.AddAsync(role);
                logger.LogInformation(
                    "{RoleType} '{RoleName}' (ID: {RoleId}) has been created.",
                    typeof(TRole).Name, role.Name, role.Id
                    );
            }

            var user = await Users.FindAsync(SysGuids.User.Administrator);
            if (user == null)
            {
                var email = "Admin@company.com";

                user = new TUser
                {
                    Id = SysGuids.User.Administrator,
                    Email = email,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = hasher.HashPassword(user!, "Administrat0r!"),
                    UserName = email
                };
                ((AuthUser)user).SetClaims(role.Name!);

                await Users.AddAsync(user);
                logger.LogInformation(
                    "{UserType} '{UserEmail}' (ID: {UserId}) has been created.",
                    typeof(TUser).Name, user.Email, user.Id
                    );
            }

            try
            {
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SaveChangesAsync() method failed.");
            }
        }
    }
}
