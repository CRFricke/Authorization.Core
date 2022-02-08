using System.Linq;
using System.Security.Claims;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="ClaimsPrincipal"/> class.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the user ID associated with the ClaimsPrincipal (<em>null</em>, if the ID cannot be located on the ClaimsPrincipal).
        /// </summary>
        /// <param name="claimsPrincipal">The ClaimsPrincipal of the user.</param>
        /// <returns>The user ID associated with the ClaimsPrincipal.</returns>
        public static string? UserId(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;

        /// <summary>
        /// Returns the user name associated with the ClaimsPrincipal ("&lt;unknown&gt;", if the ID cannot be located on the ClaimsPrincipal).
        /// </summary>
        /// <param name="claimsPrincipal">The ClaimsPrincipal of the user.</param>
        /// <returns>The user name associated with the ClaimsPrincipal.</returns>
        public static string UserName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Identity?.Name ?? "<unknown>";
    }
}
