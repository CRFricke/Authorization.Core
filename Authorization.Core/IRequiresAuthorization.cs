namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Interface used to mark application objects that require authorization.
    /// </summary>
    public interface IRequiresAuthorization
    {
        /// <summary>
        /// The ID of the object requiring authorization.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Tha name of the object requiring authorization.
        /// </summary>
        string Name { get; }
    }
}
