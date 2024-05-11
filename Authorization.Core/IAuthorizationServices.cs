using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Defines the service methods exposed by the <see cref="AuthorizationManager"/> class.
/// </summary>
public interface IAuthorizationServices
{
    /// <summary>
    /// Returns the claims associated with the specified Roles.
    /// </summary>
    /// <param name="roleIds">
    /// A HashSet containing the IDs of the Roles whose claims are to be retrieved.
    /// </param>
    /// <returns>A HashSet containing the associated claims.</returns>
    Task<HashSet<string>> GetRoleClaimsAsync(HashSet<string> roleIds);

    /// <summary>
    /// Returns the claims associated with the specified Role.
    /// </summary>
    /// <param name="roleId">The ID of the Role whose claims are to be retrieved.</param>
    /// <returns>A HashSet containing the associated claims.</returns>
    Task<HashSet<string>> GetRoleClaimsAsync(string roleId);

    /// <summary>
    /// Returns the IDs of any Roles assigned to the specified user.
    /// </summary>
    /// <param name="userId">The Id of the user whose Role IDs are to be returned.</param>
    /// <returns>A HashSet containing the IDs of the assigned roles.</returns>
    Task<HashSet<string>> GetUserRolesAsync(string userId);
}