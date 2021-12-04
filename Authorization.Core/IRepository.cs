using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Defines the methods and properties exposed by the database repository.
    /// </summary>
    /// <typeparam name="TUser">The <see cref="Type"/> of user objects. The Type must be or extend from <see cref="AuthUser"/>.</typeparam>
    /// <typeparam name="TRole">The <see cref="Type"/> of role objects. The Type must be or extend from <see cref="AuthRole"/>.</typeparam>
    public interface IRepository<TUser, TRole>
        where TUser : AuthUser
        where TRole : AuthRole
    {
        /// <summary>
        /// The collection of roles contained in the database repository.
        /// </summary>
        DbSet<TRole> Roles { get; }

        /// <summary>
        /// The collection of users contained in the database repository.
        /// </summary>
        DbSet<TUser> Users { get; }

        /// <summary>
        /// The collection of role claims contained in the database repository.
        /// </summary>
        DbSet<IdentityRoleClaim<string>> RoleClaims { get; }

        /// <summary>
        /// The collection of user claims contained in the database repository.
        /// </summary>
        DbSet<IdentityUserClaim<string>> UserClaims { get; }

        /// <summary>
        /// Creates a <see cref="DbSet{TEntity}"/> that can be used to query and save instances of TEntity.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be returned.</typeparam>
        /// <returns>A set for the given entity type.</returns>
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        /// <summary>
        /// Saves all changes made in this context to the repository.Use 'await' to ensure that 
        /// any asynchronous operations have completed before calling another method on this context.
        /// </summary>
        /// <param name="cancellationToken">
        /// A System.Threading.CancellationToken to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous save operation. The task result contains
        /// the number of state entries written to the database.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
