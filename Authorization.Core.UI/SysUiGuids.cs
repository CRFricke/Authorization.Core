using System.Collections.Generic;

namespace CRFricke.Authorization.Core.UI
{
    /// <summary>
    /// Defines the Guids used by the Authorization system UI.
    /// </summary>
    public class SysUiGuids
    {
        /// <summary>
        /// The Guids of the system Roles.
        /// </summary>
        public class Role : IDefinesGuids
        {
            /// <summary>
            /// The Guid assigned to the RoleManager Role.
            /// </summary>
            public const string RoleManager = "5e79c59c-b0c1-4857-8f3a-d99dbd1e099f";

            /// <summary>
            /// The Guid assigned to the UserManager Role.
            /// </summary>
            public const string UserManager = "d29ad18a-eaae-407c-8398-92a99182148a";

            /// <summary>
            /// Returns a list of all GUIDs defined for Role entities.
            /// </summary>
            public static readonly List<string> DefinedGuids = new()
            {
                RoleManager, UserManager
            };

            ///<inheritdoc/>
            List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
        }
    }
}
