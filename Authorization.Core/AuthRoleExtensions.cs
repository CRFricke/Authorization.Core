using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Provides extension methods for manipulating <see cref="AuthRole"/> objects.
    /// </summary>
    public static class AuthRoleExtensions
    {
        /// <summary>
        /// Sets the Claims collection of this <see cref="AuthRole"/> object.
        /// </summary>
        /// <param name="role">The <see cref="AuthRole"/> whose Claims collection is to be updated.</param>
        /// <param name="claims">The claim values to be assigned to this application role.</param>
        public static TRole SetClaims<TRole>(this TRole role, IEnumerable<string> claims) where TRole : AuthRole
        {
            return SetClaims(role, claims.ToArray());
        }

        /// <summary>
        /// Sets the Claims collection of this <see cref="AuthRole"/> object.
        /// </summary>
        /// <param name="role">The <see cref="AuthRole"/> whose Claims collection is to be updated.</param>
        /// <param name="claims">The claim values to be assigned to this application role.</param>
        public static TRole SetClaims<TRole>(this TRole role, params string[] claims) where TRole : AuthRole
        {
            role.Claims.Clear();

            foreach (var claim in claims)
            {
                role.Claims.Add(SysClaims.CreateRoleClaim(roleId: role.Id, claimValue: claim));
            }

            return role;
        }

        /// <summary>
        /// Updates the Claims collection using the specified claim values.
        /// </summary>
        /// <param name="role">The <see cref="AuthRole"/> whose Claims collection is to be updated.</param>
        /// <param name="assignedClaims">The claim values to be assigned to this application role.</param>
        /// <returns><em>true</em>, if the Claims collection was modified; otherwise, <em>false</em>.</returns>
        public static bool UpdateClaims(this AuthRole role, IEnumerable<string> assignedClaims)
        {
            return UpdateClaims(role, assignedClaims.ToArray());
        }

        /// <summary>
        /// Updates the Claims collection using the specified claim values.
        /// </summary>
        /// <param name="role">The <see cref="AuthRole"/> whose Claims collection is to be updated.</param>
        /// <param name="assignedClaims">The claim values to be assigned to this application role.</param>
        /// <returns><em>true</em>, if the Claims collection was modified; otherwise, <em>false</em>.</returns>
        public static bool UpdateClaims(this AuthRole role, params string[] assignedClaims)
        {
            var oldClaims =
                from claim in role.Claims
                select claim.ClaimValue;

            // Linq doesn't run queries until the results are needed. We use ToArray() below to force query 
            // execution, which prevents an InvalidOperationException while enumerating the result sets.
            var claimsInCommon = oldClaims.Intersect(assignedClaims);
            var claimsToAdd = assignedClaims.Except(claimsInCommon).ToArray();
            var claimsToRemove = oldClaims.Except(claimsInCommon).ToArray();

            foreach (var claim in claimsToRemove)
            {
                role.Claims.Remove(
                    role.Claims.Where(c => c.ClaimValue == claim).First()
                    );
            }

            foreach (var claim in claimsToAdd)
            {
                role.Claims.Add(
                    new IdentityRoleClaim<string> { RoleId = role.Id, ClaimType = SysClaims.ClaimType, ClaimValue = claim }
                    );
            }

            return claimsToAdd.Length != 0 || claimsToRemove.Length != 0;
        }
    }
}
