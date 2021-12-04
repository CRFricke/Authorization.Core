using System.Collections.Generic;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Used to identify the classes that define the application's Claims.
    /// </summary>
    public interface IDefinesClaims
    {
        /// <summary>
        /// Returns all Claims defined for a resource.
        /// </summary>
        List<string> DefinedClaims { get; }
    }
}
