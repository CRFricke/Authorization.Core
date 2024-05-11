using System.Security.Claims;

namespace CRFricke.Authorization.Core;

/// <summary>
/// The AuthorizationManager context supplied to 
/// <see cref="IResourceHandler.HandleAsync(ResourceHandlerContext)"/> methods.
/// </summary>
/// <param name="authorizationServices">
/// The <see cref="IAuthorizationServices"/> exposed by the <see cref="AuthorizationManager"/>.
/// </param>
/// <param name="resource">The resource going through authorization.</param>
/// <param name="principal">
/// A <see cref="ClaimsPrincipal"/> that identifies the user attempting to access the resource.
/// </param>
/// <param name="claimRequirements">The claims required for authorization.</param>
public class ResourceHandlerContext(
    IAuthorizationServices authorizationServices,
    object resource,
    ClaimsPrincipal principal,
    AppClaimRequirement claimRequirements
    )
{
    /// <summary>
    /// Service methods exposed by the <see cref="AuthorizationManager"/> class.
    /// </summary>
    public IAuthorizationServices AuthorizationServices { get; } = authorizationServices;

    /// <summary>
    /// A <see cref="ClaimsPrincipal"/> that identifies the user attempting to access the resource.
    /// </summary>
    public ClaimsPrincipal Principal { get; } = principal;

    /// <summary>
    /// The resource going through authorization.
    /// </summary>
    public object Resource { get; } = resource;

    /// <summary>
    /// The claims required for authorization.
    /// </summary>
    public AppClaimRequirement ClaimRequirements { get; } = claimRequirements;
}
