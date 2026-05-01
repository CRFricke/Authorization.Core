using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Provides extension methods for manipulating <see cref="AuthRole"/> objects.
/// </summary>
public static class AuthRoleExtensions
{
    extension(AuthRole role)
    {
        /// <summary>
        /// Sets the Claims collection of this <see cref="AuthRole"/> object.
        /// </summary>
        /// <param name="claims">The claim values to be assigned to this application role.</param>
        public TRole SetClaims<TRole>(params IEnumerable<string> claims) where TRole : AuthRole
        {
            ArgumentNullException.ThrowIfNull(claims);

            var claimsArray = claims as string[] ?? [.. claims];

            role.Claims.Clear();

            foreach (var claim in claimsArray)
            {
                role.Claims.Add(SysClaims.CreateRoleClaim(roleId: role.Id, claimValue: claim));
            }

            return (TRole)role;
        }

        /// <summary>
        /// Updates the Claims collection using the specified claim values.
        /// </summary>
        /// <param name="assignedClaims">The claim values to be assigned to this application role.</param>
        /// <returns><em>true</em>, if the Claims collection was modified; otherwise, <em>false</em>.</returns>
        public bool UpdateClaims(params IEnumerable<string> assignedClaims)
        {
            var claimsArray = assignedClaims as string[] ?? [.. assignedClaims];

            var oldClaims = (
                from claim in role.Claims
                select claim.ClaimValue).ToArray();

            // Linq doesn't run queries until the results are needed. We use ToArray() below to force query 
            // execution, which prevents an InvalidOperationException while enumerating the result sets.
            var claimsInCommon = oldClaims.Intersect(claimsArray).ToArray();
            var claimsToAdd = claimsArray.Except(claimsInCommon).ToArray();
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
