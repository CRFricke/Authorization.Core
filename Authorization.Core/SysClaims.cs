using CRFricke.Authorization.Core.Attributes;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Defines the Claims used by the Authorization system.
    /// </summary>
    public class SysClaims
    {
        /// <summary>
        /// The claim type of the claims the Authorization.Core library uses.
        /// </summary>
        public const string ClaimType = ClaimTypes.AuthorizationDecision;


        /// <summary>
        /// Creates a new IdentityRoleClaim using the specified Role ID and claim value.
        /// </summary>
        /// <param name="roleId">The ID of the Role being assigned the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>A new IdentityRoleClaim with the specified values.</returns>
        public static IdentityRoleClaim<string> CreateRoleClaim(string roleId, string claimValue)
            => new IdentityRoleClaim<string> { RoleId = roleId, ClaimType = ClaimType, ClaimValue = claimValue };

        /// <summary>
        /// Creates a new IdentityUserClaim using the specified User ID and claim value.
        /// </summary>
        /// <param name="userId">The ID of the User being assigned the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>A new IdentityUserClaim with the specified values.</returns>
        public static IdentityUserClaim<string> CreateUserClaim(string userId, string claimValue)
            => new IdentityUserClaim<string> { UserId = userId, ClaimType = ClaimTypes.Role, ClaimValue = claimValue };


        /// <summary>
        /// Defines the claims associated with the Role entity.
        /// </summary>
        public class Role : IDefinesClaims
        {
            /// <summary>
            /// The user can create Role entities.
            /// </summary>
            public const string Create = "Role.Create";

            /// <summary>
            /// The user can read Role entities.
            /// </summary>
            public const string Read = "Role.Read";

            /// <summary>
            /// The user can update Role entities.
            /// </summary>
            public const string Update = "Role.Update";

            /// <summary>
            /// The user can delete Role entities.
            /// </summary>
            [RestrictedClaim]
            public const string Delete = "Role.Delete";

            /// <summary>
            /// The user can list Role entities.
            /// </summary>
            public const string List = "Role.List";

            /// <summary>
            /// The user can update the claims collection of Role entities.
            /// </summary>
            [RestrictedClaim]
            public const string UpdateClaims = "Role.UpdateClaims";

            /// <summary>
            /// Returns a list of all Claims defined for Role entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Create, Delete, Read, Update, List, UpdateClaims
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }

        /// <summary>
        /// Defines the claims associated with the User entity.
        /// </summary>
        public class User : IDefinesClaims
        {
            /// <summary>
            /// The user can create User entities.
            /// </summary>
            public const string Create = "User.Create";

            /// <summary>
            /// The user can read User entities.
            /// </summary>
            public const string Read = "User.Read";

            /// <summary>
            /// The user can update User entities.
            /// </summary>
            public const string Update = "User.Update";

            /// <summary>
            /// The user can delete User entities.
            /// </summary>
            [RestrictedClaim]
            public const string Delete = "User.Delete";

            /// <summary>
            /// The user can list User entities.
            /// </summary>
            public const string List = "User.List";

            /// <summary>
            /// The user can update the claims collection of User entities.
            /// </summary>
            [RestrictedClaim]
            public const string UpdateClaims = "User.UpdateClaims";

            /// <summary>
            /// Returns a list of all Claims defined for User entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Create, Delete, Read, Update, List, UpdateClaims
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }
    }
}
