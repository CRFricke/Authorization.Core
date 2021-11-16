using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Fricke.Authorization.Core.Data
{
    /// <summary>
    /// Base class for application users.
    /// </summary>
    public class AuthUser : IdentityUser, IRequiresAuthorization
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthUser"/> class.
        /// </summary>
        public AuthUser()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="AuthUser"/> class with the specified email address.
        /// </summary>
        /// <param name="email">The email address of the new <see cref="AuthUser"/>.</param>
        public AuthUser(string email) : this(email, email)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="AuthUser"/> class with the specified email address and user name.
        /// </summary>
        /// <param name="email">The email address of the new <see cref="AuthUser"/>.</param>
        /// <param name="userName">The user name of the new <see cref="AuthUser"/>.</param>
        public AuthUser(string email, string userName) : base(userName)
        {
            Email = email;
        }

        ///<inheritdoc/>
        string IRequiresAuthorization.Name => Email;

        /// <summary>
        /// The claims that this application user possesses.
        /// </summary>
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; } = new List<IdentityUserClaim<string>>();
    }
}
