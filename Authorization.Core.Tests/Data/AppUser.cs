using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.Data;

#pragma warning disable CA1515 // Consider making public types internal

namespace Authorization.Core.Tests.Data;

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

    /// <summary>
    /// Creates a new instance of the AppUser class with the specified user name.
    /// </summary>
    /// <param name="userName">The user name of the new AppUser.</param>
    public AppUser(string userName) : base(userName)
    { }
}
