using CRFricke.Authorization.Core.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Performs additional authorization checks for a specified Role.
/// </summary>
/// <typeparam name="TRole">The type of application's role objects.</typeparam>
public class RoleAuthorizationHandler<TRole> : IResourceAuthorizationHandler<TRole>
    where TRole : AuthRole, new()
{
    ///<inheritdoc/>
    public async Task<AuthorizationResult> HandleAsync(ResourceAuthorizationHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ClaimRequirements.ClaimValues.Contains(SysClaims.Role.Delete))
        {
            // Nothing to check if the Role is being deleted (we've already 
            // checked that the user has the 'Role.Delete' claim).
            return AuthorizationResult.Success();
        }

        if (context.Resource is null || context.Resource is not TRole role)
        {
            return AuthorizationResult.Success();
        }

        // Principal.UserId() is verified before the ResourceAuthorizationHandler is called.
        var principalId = context.Principal.UserId()!;

        var principalRoles = await context.AuthorizationServices.GetUserRolesAsync(principalId);
        if (principalRoles.Contains(SysGuids.Role.Administrator))
        {
            return AuthorizationResult.Success();
        }

        var principalClaims = await context.AuthorizationServices.GetRoleClaimsAsync(principalRoles);
        var roleClaims = await context.AuthorizationServices.GetRoleClaimsAsync(role.Id);

        if (roleClaims.IsSubsetOf(principalClaims))
        {
            return AuthorizationResult.Success();
        }

        return AuthorizationResult.Elevation(
            roleClaims.Except(principalClaims)
            );
    }
}
