using CRFricke.Authorization.Core;
using System.Collections.Generic;

namespace Authorization.Core.UI.Tests.Web.Authorization
{
    /// <summary>
    /// Defines the Guids used by the application.
    /// </summary>
    public class AppGuids
    {
        /// <summary>
        /// The Guids of the application's default roles.
        /// </summary>
        public class Role : IDefinesGuids
        {
            /// <summary>
            /// The Guid assigned to the CalendarManager role.
            /// </summary>
            public const string CalendarManager = "fee71d38-9ed2-4962-8f43-8cd48678c65e";

            /// <summary>
            /// The Guid assigned to the DocumentManager role.
            /// </summary>
            public const string DocumentManager = "45d88a4c-a4d9-4c2a-8cbf-38c883ff6130";

            /// <summary>
            /// Returns a list of all GUIDs defined for role entities.
            /// </summary>
            public static readonly List<string> DefinedGuids = new List<string>
            {
                CalendarManager, DocumentManager
            };

            ///<inheritdoc/>
            List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
        }

        /// <summary>
        /// The Guids of the application's default users.
        /// </summary>
        public class User : IDefinesGuids
        {
            /// <summary>
            /// The Guid assigned to the <see cref="CalendarGuy"/> user account.
            /// </summary>
            public const string CalendarGuy = "46453d18-c3c9-4e1c-8f97-99cc321995cc";

            /// <summary>
            /// The Guid assigned to the <see cref="DocumentGuy"/> user account.
            /// </summary>
            public const string DocumentGuy = "cbf10529-dc73-4ed8-862e-0969df6485a9";

            /// <summary>
            /// Returns a list of all GUIDs defined for user entities.
            /// </summary>
            public static readonly List<string> DefinedGuids = new List<string>
            {
                CalendarGuy, DocumentGuy
            };

            ///<inheritdoc/>
            List<string> IDefinesGuids.DefinedGuids => DefinedGuids;
        }

    }
}
