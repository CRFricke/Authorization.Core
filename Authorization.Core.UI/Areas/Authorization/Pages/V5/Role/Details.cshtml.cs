using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.Role
{
    [RequiresClaims(SysClaims.Role.Read)]
    [PageImplementationType(typeof(DetailsModel<,>))]
    public abstract class DetailsModel : ModelBase
    {
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id)
            => throw new NotImplementedException();
    }

    internal class DetailsModel<TUser, TRole> : DetailsModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly DetailsHandler<TUser, TRole> _detailsHandler;

        /// <summary>
        /// Creates a new DetailsModel&lt;TUser, TRole&gt; class instance using the specified <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsModel.</param>
        public DetailsModel(IServiceProvider serviceProvider)
        {
            _detailsHandler = new DetailsHandler<TUser, TRole>(serviceProvider);
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            RoleModel = new RoleModel();
            return await _detailsHandler.OnGetAsync(RoleModel, this, id);
        }
    }
}
