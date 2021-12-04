using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CRFricke.Authorization.Core.Data
{
    /// <summary>
    /// Base class for application roles.
    /// </summary>
    public class AuthRole : IdentityRole, IRequiresAuthorization
    {
        /// <summary>
        /// The claims that have been granted to this application role.
        /// </summary>
        public virtual ICollection<IdentityRoleClaim<string>> Claims { get; } = new List<IdentityRoleClaim<string>>();
    }
}
