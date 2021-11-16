using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Fricke.Authorization.Core
{
    /// <summary>
    /// Represents an authorization requirement for one or more application claim values.
    /// </summary>
    public class AppClaimRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Creates a new instance of the AppClaimRequirement class using the specified claim values.
        /// </summary>
        /// <param name="claimValues">The required claim value(s).</param>
        public AppClaimRequirement(params string[] claimValues)
        {
            ClaimValues = claimValues.ToHashSet();
        }

        /// <summary>
        /// The claim type of the associated application claims.
        /// </summary>
        public const string ClaimType = ClaimTypes.AuthorizationDecision;

        /// <summary>
        /// The claim value(s) associated with this AuthorizationRequirement.
        /// </summary>
        public HashSet<string> ClaimValues { get; }

        public override string ToString()
        {
            return $"{string.Join(", ", ClaimValues)}";
        }
    }
}
