using Microsoft.AspNetCore.Authorization;
using System;

namespace Fricke.Authorization.Core.Attributes
{
    ///<inheritdoc/>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresClaimAttribute : AuthorizeAttribute
    {
        internal const string PolicyDelimeter = ": ";
        internal const string PolicyHeader = "RequiresClaim";
        internal const string ValueDelimeter = ", ";

        /// <summary>
        /// Creates a new instance of the RequiresClaimAttribute class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private RequiresClaimAttribute()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        /// <summary>
        /// Creates a new instance of the RequiresClaimAttribute class with the specified claim type and values.
        /// </summary>
        /// <param name="claimType">The claim type to be used to initialize the new RequiresClaimAttribute instance.</param>
        /// <param name="claimValues">The claim values to be used to initialize the new RequiresClaimAttribute instance.</param>
        public RequiresClaimAttribute(params string[] claimValues)
        {
            ClaimValues = claimValues;

            Policy = $"{PolicyHeader}{PolicyDelimeter}{string.Join(ValueDelimeter, claimValues)}";
        }


        /// <summary>
        /// Gets the required claim values.
        /// </summary>
        public string[] ClaimValues { get; private set; }


        /// <summary>
        /// Attempts to parse the specified policy name into a new RequiresClaimAttribute class instance.
        /// </summary>
        /// <param name="policyName">The policy name string to be parsed.</param>
        /// <param name="requiresClaimAttribute">If successful, the new RequiresClaimAttribute instance; otherwise, <code>null</code>.</param>
        /// <returns><em>true</em>, if the parse operation was successful; otherwise, <em>false</em>.</returns>
        internal static bool TryParse(string policyName, out RequiresClaimAttribute? requiresClaimAttribute)
        {
            var segments = policyName.Split(PolicyDelimeter);
            if (segments.Length == 2)
            {
                if (segments[0] == PolicyHeader)
                {
                    requiresClaimAttribute = new RequiresClaimAttribute
                    {
                        ClaimValues = segments[1].Split(ValueDelimeter),
                        Policy = policyName
                    };

                    return true;
                }
            }

            requiresClaimAttribute = null;
            return false;
        }
    }
}
