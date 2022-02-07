using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Authorization handler for AppClaimRequirements
    /// </summary>
    public class AppClaimRequirementHandler : AuthorizationHandler<AppClaimRequirement, object>
    {
        private readonly IAuthorizationManager _authorizationManager;

        /// <summary>
        /// Creates a new instance of the AppClaimRequirementHandler class using the specified AuthorizationManager.
        /// </summary>
        /// <param name="authorizationManager">The <see cref="IAuthorizationManager"/> to be used for evaluating AppClaimRequirements.</param>
        public AppClaimRequirementHandler(IAuthorizationManager authorizationManager)
        {
            _authorizationManager = authorizationManager;
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AppClaimRequirement requirement, object resource)
        {
            AuthorizationResult result;

            if (resource is not IRequiresAuthorization)
            {
                result = await _authorizationManager.AuthorizeAsync(context.User, requirement);
                if (result.Succeeded)
                {
                    context.Succeed(requirement);
                }

                return;
            }

            result = await _authorizationManager.AuthorizeAsync(context.User, resource, requirement);
            if (result.Succeeded)
            {
                context.Succeed(requirement);
            }

            return;
        }
    }
}
