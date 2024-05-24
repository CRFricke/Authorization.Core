using System.Security.Claims;

namespace CRFricke.Authorization.Core;

/// <summary>
/// Contains authorization context information for use by 
/// <see cref="IResourceAuthorizationHandler"/> implementations.
/// </summary>
public class ResourceAuthorizationHandlerContext
{
    /// <param name="authorizationServices">
    /// The <see cref="IAuthorizationServices"/> exposed by the <see cref="AuthorizationManager"/>.
    /// </param>
    /// <param name="resource">The resource going through authorization.</param>
    /// <param name="principal">
    /// A <see cref="ClaimsPrincipal"/> that identifies the user attempting to access the resource.
    /// </param>
    /// <param name="claimRequirements">The claims required for authorization.</param>
    internal ResourceAuthorizationHandlerContext(
        IAuthorizationServices authorizationServices,
        IRequiresAuthorization resource,
        ClaimsPrincipal principal,
        AppClaimRequirement claimRequirements
        )
    {
        AuthorizationServices = authorizationServices;
        Principal = principal;
        Resource = resource;
        ClaimRequirements = claimRequirements;
    }

    /// <summary>
    /// Service methods exposed by the <see cref="AuthorizationManager"/> class.
    /// </summary>
    public IAuthorizationServices AuthorizationServices { get; }

    /// <summary>
    /// A <see cref="ClaimsPrincipal"/> that identifies the user attempting to access the resource.
    /// </summary>
    public ClaimsPrincipal Principal { get; }

    /// <summary>
    /// The resource going through authorization.
    /// </summary>
    public IRequiresAuthorization Resource { get; }

    /// <summary>
    /// The claims required for authorization.
    /// </summary>
    public AppClaimRequirement ClaimRequirements { get; }
}
