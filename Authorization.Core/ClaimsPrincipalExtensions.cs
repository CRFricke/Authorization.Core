using System.Linq;
using System.Security.Claims;

namespace Fricke.Authorization.Core
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the user ID associated with the ClaimsPrincipal (<em>null</em>, if the ID cannot be located on the ClaimsPrincipal).
        /// </summary>
        /// <param name="claimsPrincipal">The ClaimsPrincipal of the user.</param>
        /// <returns>The user ID associated with the ClaimsPrincipal.</returns>
        public static string? UserId(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value;
    }
}
