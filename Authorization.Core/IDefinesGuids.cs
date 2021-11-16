using System.Collections.Generic;

namespace Fricke.Authorization.Core
{
    /// <summary>
    /// Used to identify the classes that define the application's GUIDs.
    /// </summary>
    public interface IDefinesGuids
    {
        /// <summary>
        /// Returns all GUIDs defined for a resource.
        /// </summary>
        List<string> DefinedGuids { get; }
    }
}
