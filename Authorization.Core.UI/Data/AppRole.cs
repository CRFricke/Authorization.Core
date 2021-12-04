using CRFricke.Authorization.Core.Data;

namespace CRFricke.Authorization.Core.UI.Data
{
    public class AppRole : AuthRole
    {
        /// <summary>
        /// A description of the role.
        /// </summary>
        public virtual string Description { get; set; }
    }
}
