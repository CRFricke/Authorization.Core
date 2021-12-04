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
        public AuthDbContext(DbContextOptions options) : base(options)
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
        public AuthDbContext(DbContextOptions options) : base(options)
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
    }
}
