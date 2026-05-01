namespace CRFricke.Authorization.Core;

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1034 // Nested types should not be visible

/// <summary>
/// Defines the Guids used by the Authorization system.
/// </summary>
public static class SysGuids
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
        public static readonly IReadOnlyCollection<string> DefinedGuids =
        [
            Administrator
        ];

        ///<inheritdoc/>
        IReadOnlyCollection<string> IDefinesGuids.DefinedGuids => DefinedGuids;
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
        public static readonly IReadOnlyCollection<string> DefinedGuids =
        [
            Administrator
        ];

        ///<inheritdoc/>
        IReadOnlyCollection<string> IDefinesGuids.DefinedGuids => DefinedGuids;
    }
}
