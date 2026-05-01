using CRFricke.Authorization.Core;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable CA1724 // Type names should not match namespaces

namespace Authorization.Core.UI.Test.Web;

/// <summary>
/// Defines the Claims used by the application.
/// </summary>
public sealed class AppClaims
{
    /// <summary>
    /// Defines the claims required to manipulate Calendar events.
    /// </summary>
    public sealed class Calendar : IDefinesClaims
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
        public static readonly IReadOnlyCollection<string> DefinedClaims =
        [
            Create, Delete, Read, Update, List
        ];

        ///<inheritdoc/>
        IReadOnlyCollection<string> IDefinesClaims.DefinedClaims => DefinedClaims;
    }

    /// <summary>
    /// Defines the claims required to manipulate Documents.
    /// </summary>
    public sealed class Document : IDefinesClaims
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
        public static readonly IReadOnlyCollection<string> DefinedClaims =
        [
            Upload, Delete, Read, List
        ];

        ///<inheritdoc/>
        IReadOnlyCollection<string> IDefinesClaims.DefinedClaims => DefinedClaims;
    }
}
