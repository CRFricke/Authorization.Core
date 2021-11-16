using Fricke.Authorization.Core;
using Fricke.Authorization.Core.Data;

namespace Authorization.Core.Tests.Data
{
    /// <summary>
    ///  Describes an application role entity.
    /// </summary>
    public class AppRole : AuthRole, IRequiresAuthorization
    { }
}
