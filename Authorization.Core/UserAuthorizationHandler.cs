using CRFricke.Authorization.Core.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Performs additional authorization checks for a specified User.
/// </summary>
/// <typeparam name="TUser">The type of application's user objects.</typeparam>
public class UserAuthorizationHandler<TUser> : IResourceAuthorizationHandler<TUser>
    where TUser : AuthUser, new()
{
    ///<inheritdoc/>
    public async Task<AuthorizationResult> HandleAsync(ResourceAuthorizationHandlerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ClaimRequirements.ClaimValues.Contains(SysClaims.User.Delete))
        {
            // Nothing to check if the User is being deleted (we've already 
            // checked that the user has the 'User.Delete' claim).
            return AuthorizationResult.Success();
        }

        if (context.Resource is null || context.Resource is not TUser user)
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

        var userRoles = await context.AuthorizationServices.GetUserRolesAsync(user.Id);
        if (userRoles.Contains(SysGuids.Role.Administrator))
        {
            return AuthorizationResult.Elevation([nameof(SysGuids.Role.Administrator)]);
        }

        var principalClaims = await context.AuthorizationServices.GetRoleClaimsAsync(principalRoles);
        var userClaims = await context.AuthorizationServices.GetRoleClaimsAsync(userRoles);

        if (userClaims.IsSubsetOf(principalClaims))
        {
            return AuthorizationResult.Success();
        };

        return AuthorizationResult.Elevation(
            userClaims.Except(principalClaims)
            );
    }
}
