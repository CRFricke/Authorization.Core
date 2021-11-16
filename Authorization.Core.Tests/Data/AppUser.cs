using Fricke.Authorization.Core;
using Fricke.Authorization.Core.Data;

namespace Authorization.Core.Tests.Data
{
    /// <summary>
    ///  Describes an application user entity.
    /// </summary>
    public class AppUser : AuthUser, IRequiresAuthorization
    {
        /// <summary>
        /// Creates a new instance of the AppUser class with default values.
        /// </summary>
        public AppUser()
        { }

        /// Creates a new instance of the AppUser class with the specified user name.
        /// </summary>
        /// <param name="userName">The user name of the new AppUser.</param>
        public AppUser(string userName) : base(userName)
        { }
    }
}
