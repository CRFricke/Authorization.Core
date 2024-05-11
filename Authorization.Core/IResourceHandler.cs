using System.Threading.Tasks;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Performs additional authorization checks for a specified resource.
/// </summary>
public interface IResourceHandler 
{
    /// <summary>
    /// Called to handle any additional authorization checks required for a resource.
    /// </summary>
    /// <param name="context">
    /// A <see cref="ResourceHandlerContext"/> providing context for the authorization check.
    /// </param>
    /// <returns>
    /// A <see cref="AuthorizationResult"/> object describing the result of the authorization check.
    /// </returns>
    Task<AuthorizationResult> HandleAsync(ResourceHandlerContext context);
}

/// <summary>
/// Performs additional authorization checks for a specified resource.
/// </summary>
/// <typeparam name="TResource">The type of the application resource being checked.</typeparam>
public interface IResourceHandler<TResource> : IResourceHandler
    where TResource : class, IRequiresAuthorization
{ }

