using Fricke.Authorization.Core;
using Fricke.Authorization.Core.UI.Data;

namespace Authorization.Core.UI.Tests.Data
{
    public class ApplicationUser : AppUser, IRequiresAuthorization
    {
        /// <summary>
        /// Creates a new instance of the ApplicationUser class with default values.
        /// </summary>
        public ApplicationUser()
        { }

        /// Creates a new instance of the AppUser class with the specified user name.
        /// </summary>
        /// <param name="userName">The user name of the new AppUser.</param>
        public ApplicationUser(string userName) : base(userName)
        { }

        string IRequiresAuthorization.Name => !string.IsNullOrEmpty(DisplayName) ? DisplayName : Email;
    }
}
