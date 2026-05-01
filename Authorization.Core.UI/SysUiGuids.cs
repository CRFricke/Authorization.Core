namespace CRFricke.Authorization.Core.UI;

#pragma warning disable CA1033 // Interface methods should be callable by child types
#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1724 // Type names should not match namespaces

/// <summary>
/// Defines the Guids used by the Authorization system UI.
/// </summary>
public static class SysUiGuids
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
        public static readonly IReadOnlyCollection<string> DefinedGuids =
        [
            RoleManager, UserManager
        ];

        ///<inheritdoc/>
        IReadOnlyCollection<string> IDefinesGuids.DefinedGuids => DefinedGuids;
    }
}
