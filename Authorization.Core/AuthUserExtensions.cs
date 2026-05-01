using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Provides extension methods for manipulating <see cref="AuthUser"/> objects.
/// </summary>
public static class AuthUserExtensions
{
    extension(AuthUser user)
    {
        /// <summary>
        /// Sets the Claims collection of this <see cref="AuthUser"/> objects.
        /// </summary>
        /// <param name="claims">The claim values to be assigned to this <see cref="AuthUser"/>.</param>
        /// <returns>The <typeparamref name="TUser"/>class instance.</returns>
        public TUser SetClaims<TUser>(params IEnumerable<string> claims) where TUser : AuthUser
        {
            ArgumentNullException.ThrowIfNull(claims);

            var claimsArray = claims as string[] ?? [.. claims];

            user.Claims.Clear();

            foreach (var claim in claimsArray)
            {
                user.Claims.Add(SysClaims.CreateUserClaim(userId: user.Id, claimValue: claim));
            }

            return (TUser)user;
        }

        /// <summary>
        /// Updates the Claims collection using the specified claim values.
        /// </summary>
        /// <param name="assignedClaims">The claim values to be assigned to this application user.</param>
        /// <returns><em>true</em>, if the Claims collection was modified; otherwise, <em>false</em>.</returns>
        public bool UpdateClaims(params IEnumerable<string> assignedClaims)
        {
            var claimsArray = assignedClaims as string[] ?? [.. assignedClaims];

            var oldClaims = (
                from claim in user.Claims
                select claim.ClaimValue).ToArray();

            // Linq doesn't run queries until the results are needed. We use ToArray() below to force query 
            // execution, which prevents an InvalidOperationException while enumerating the result sets.
            var claimsInCommon = oldClaims.Intersect(claimsArray).ToArray();
            var claimsToAdd = claimsArray.Except(claimsInCommon).ToArray();
            var claimsToRemove = oldClaims.Except(claimsInCommon).ToArray();

            foreach (var claim in claimsToRemove)
            {
                user.Claims.Remove(
                    user.Claims.Where(c => c.ClaimValue == claim).First()
                    );
            }

            foreach (var claim in claimsToAdd)
            {
                user.Claims.Add(
                    new IdentityUserClaim<string> { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = claim }
                    );
            }

            return claimsToAdd.Length != 0 || claimsToRemove.Length != 0;
        }
    }
}
