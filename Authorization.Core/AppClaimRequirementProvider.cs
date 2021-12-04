using CRFricke.Authorization.Core.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// IAuthorizationPolicyProvider implementation that supports use of the RequiresClaims authorization attribute
    /// </summary>
    public class AppClaimRequirementProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

        /// <summary>
        /// Creates a new instance of the AppClaimRequirementProvider using the specified AuthorizationOptions.
        /// </summary>
        /// <param name="options">The AuthorizationOptions to be used to initialize the new AppClaimRequirementProvider instance.</param>
        public AppClaimRequirementProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            => FallbackPolicyProvider.GetDefaultPolicyAsync();

        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
            => FallbackPolicyProvider.GetFallbackPolicyAsync();

        /// <inheritdoc />
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            RequiresClaimsAttribute.TryParse(policyName, out RequiresClaimsAttribute? requiresClaimsAttribute);
            if (requiresClaimsAttribute != null)
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(
                    new AppClaimRequirement(requiresClaimsAttribute.ClaimValues)
                    );
                return Task.FromResult(policy.Build());
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }
    }
}
