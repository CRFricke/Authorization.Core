using Fricke.Authorization.Core.Data;

namespace Fricke.Authorization.Core.UI.Data
{
    public class AppRole : AuthRole
    {
        /// <summary>
        /// A description of the role.
        /// </summary>
        public virtual string Description { get; set; }
    }
}
