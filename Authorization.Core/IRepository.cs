using Fricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Fricke.Authorization.Core
{
    public interface IRepository<TUser, TRole>
        where TUser : AuthUser
        where TRole : AuthRole
    {
        DbSet<TRole> Roles { get; }

        DbSet<TUser> Users { get; }

        DbSet<IdentityRoleClaim<string>> RoleClaims { get; }

        DbSet<IdentityUserClaim<string>> UserClaims { get; }

        DbSet<T> Set<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
