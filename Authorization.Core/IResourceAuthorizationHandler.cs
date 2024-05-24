using System.Threading.Tasks;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Classes that implement this interface perform additional authorization checks for a specified resource.
/// </summary>
public interface IResourceAuthorizationHandler
{
    /// <summary>
    /// Called to handle any additional authorization checks required for a resource.
    /// </summary>
    /// <param name="context">
    /// A <see cref="ResourceAuthorizationHandlerContext"/> providing context for the authorization check.
    /// </param>
    /// <returns>
    /// A <see cref="AuthorizationResult"/> object describing the result of the authorization check.
    /// </returns>
    Task<AuthorizationResult> HandleAsync(ResourceAuthorizationHandlerContext context);
}

/// <summary>
/// Classes that implement this interface perform additional authorization checks for a specified resource.
/// </summary>
/// <typeparam name="TResource">The type of the application resource being checked.</typeparam>
public interface IResourceAuthorizationHandler<TResource> : IResourceAuthorizationHandler
    where TResource : class, IRequiresAuthorization
{ }

