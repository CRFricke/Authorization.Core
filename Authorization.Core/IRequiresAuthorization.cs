namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Interface used to mark application resources that require authorization.
    /// </summary>
    public interface IRequiresAuthorization
    {
        /// <summary>
        /// The ID of the resource requiring authorization.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Tha name of the resource requiring authorization.
        /// </summary>
        string? Name { get; }
    }
}
