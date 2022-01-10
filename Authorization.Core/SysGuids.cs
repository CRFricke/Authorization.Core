using System.Collections.Generic;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Defines the Guids used by the Authorization system.
    /// </summary>
    public class SysGuids
    {
        /// <summary>
        /// The Guids of the system Roles.
        /// </summary>
        public class Role : IDefinesGuids
        {
            /// <summary>
            /// The Guid assigned to the Administrator Role.
            /// </summary>
            public const string Administrator = "3f1dfcb9-7088-4877-8352-7a6e43063650";

            /// <summary>
            /// Returns a list of all GUIDs defined for Role entities.
            /// </summary>
            public static readonly List<string> DefinedGuids = new List<string>
            {
                Administrator
            };

            ///<inheritdoc/>
            List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
        }

        /// <summary>
        /// The Guids of the system Users.
        /// </summary>
        public class User : IDefinesGuids
        {
            /// <summary>
            /// The Guid assigned to the Administrator User account.
            /// </summary>
            public const string Administrator = "8156bb9b-f56e-4f83-8a11-b0418b843e9b";

            /// <summary>
            /// Returns a list of all GUIDs defined for User entities.
            /// </summary>
            public static readonly List<string> DefinedGuids = new List<string>
            {
                Administrator
            };

            ///<inheritdoc/>
            List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
        }
    }
}
