using Fricke.Authorization.Core;
using System.Collections.Generic;

namespace Authorization.Core.UI.Tests.Web.Authorization
{
    /// <summary>
    /// Defines the Claims used by the application.
    /// </summary>
    public class AppClaims
    {
        /// <summary>
        /// Defines the claims required to manipulate Bulletins.
        /// </summary>
        public class Bulletin : IDefinesClaims
        {
            /// <summary>
            /// The user can upload Bulletins.
            /// </summary>
            public const string Upload = "Bulletin.Upload";

            /// <summary>
            /// The user can read Bulletins.
            /// </summary>
            public const string Read = "Bulletin.Read";

            /// <summary>
            /// The user can delete Bulletins.
            /// </summary>
            public const string Delete = "Bulletin.Delete";

            /// <summary>
            /// The user can list Bulletins.
            /// </summary>
            public const string List = "Bulletin.List";

            /// <summary>
            /// Returns a list of all Claims defined for Bulletin entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Upload, Delete, Read, List
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }

        /// <summary>
        /// Defines the claims required to manipulate Calendar events.
        /// </summary>
        public class Calendar : IDefinesClaims
        {
            /// <summary>
            /// The user can create CalendarEvent entities.
            /// </summary>
            public const string Create = "Calendar.Create";

            /// <summary>
            /// The user can read CalendarEvent entities.
            /// </summary>
            public const string Read = "Calendar.Read";

            /// <summary>
            /// The user can update CalendarEvent entities.
            /// </summary>
            public const string Update = "Calendar.Update";

            /// <summary>
            /// The user can delete CalendarEvent entities.
            /// </summary>
            public const string Delete = "Calendar.Delete";

            /// <summary>
            /// The user can list CalendarEvent entities.
            /// </summary>
            public const string List = "Calendar.List";

            /// <summary>
            /// Returns a list of all Claims defined for Calendar entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Create, Delete, Read, Update, List
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }

        /// <summary>
        /// Defines the claims required to manipulate Documents.
        /// </summary>
        public class Document : IDefinesClaims
        {
            /// <summary>
            /// The user can upload Documents.
            /// </summary>
            public const string Upload = "Document.Upload";

            /// <summary>
            /// The user can read Documents.
            /// </summary>
            public const string Read = "Document.Read";

            /// <summary>
            /// The user can delete Documents.
            /// </summary>
            public const string Delete = "Document.Delete";

            /// <summary>
            /// The user can list Documents.
            /// </summary>
            public const string List = "Document.List";

            /// <summary>
            /// Returns a list of all Claims defined for Document entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Upload, Delete, Read, List
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }

        /// <summary>
        /// Defines the claims required to manipulate News items.
        /// </summary>
        public class News : IDefinesClaims
        {
            /// <summary>
            /// The user can create NewsItem entities.
            /// </summary>
            public const string Create = "News.Create";

            /// <summary>
            /// The user can read NewsItem entities.
            /// </summary>
            public const string Read = "News.Read";

            /// <summary>
            /// The user can update NewsItem entities.
            /// </summary>
            public const string Update = "News.Update";

            /// <summary>
            /// The user can delete NewsItem entities.
            /// </summary>
            public const string Delete = "News.Delete";

            /// <summary>
            /// The user can list NewsItem entities.
            /// </summary>
            public const string List = "News.List";

            /// <summary>
            /// Returns a list of all Claims defined for News entities.
            /// </summary>
            public static readonly List<string> DefinedClaims = new List<string>
            {
                Create, Delete, Read, Update, List
            };

            ///<inheritdoc/>
            List<string> IDefinesClaims.DefinedClaims => DefinedClaims;
        }
    }
}
