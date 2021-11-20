using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fricke.Authorization.Core
{
    /// <summary>
    /// Defines the methods and properties exposed by the <see cref="AuthorizationManager"/> class.
    /// </summary>
    public interface IAuthorizationManager
    {
        /// <summary>
        /// Returns a list of all claims defined by the application.
        /// </summary>
        List<string> DefinedClaims { get; }

        /// <summary>
        /// Returns a list of all Guids defined by the application.
        /// </summary>
        List<string> DefinedGuids { get; }

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/> 
        /// for the specified <paramref name="resource"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the authenticated user.</param>
        /// <param name="resource">The resource to be tested against.</param>
        /// <param name="claimRequirement">The requirement that must be met for authorization to be granted.</param>
        /// <returns>
        /// An <see cref="AuthorizationResult"/> specifying whether the <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </returns>
        Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, object resource, AppClaimRequirement claimRequirement);

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the authenticated user.</param>
        /// <param name="claimRequirement">The requirement that must be met for authorization to be granted.</param>
        /// <returns>
        /// An <see cref="AuthorizationResult"/> specifying whether the <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </returns>
        Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, AppClaimRequirement claimRequirement);

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> has the specified <paramref name="requiredClaims"/>. 
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the user.</param>
        /// <param name="requiredClaims">The claims that the <paramref name="principal"/> must have for authorization to be granted.</param>
        /// <returns>
        /// <em>true</em>, if the specified <paramref name="principal"/> has the specified <paramref name="requiredClaims"/>; otherwise, <em>false</em>.
        /// </returns>
        Task<bool> IsAuthorizedAsync(ClaimsPrincipal principal, params string[] requiredClaims);

        /// <summary>
        /// Removes the UserRoleCache entry of the specified user.
        /// </summary>
        /// <param name="userId">The ID of the User whose UserRoleCache entry is to be removed.</param>
        void RefreshUser(string userId);

        /// <summary>
        /// Removes the RoleClaimCache entry of the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the Role whose RoleClaimCache entry is to be removed.</param>
        void RefreshRole(string roleId);
    }
}
