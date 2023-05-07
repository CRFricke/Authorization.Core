using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role
{
    [RequiresClaims(SysClaims.Role.Update)]
    [PageImplementationType(typeof(EditModel<,>))]
    public abstract class EditModel : ModelBase
    {
        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfClaimList) => throw new NotImplementedException();
    }

    internal class EditModel<TUser, TRole> : EditModel
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        private readonly EditHandler<TUser, TRole> _editHandler;

        /// <summary>
        /// Creates a new EditModel&lt;TUser, TRole&gt; class instance using the specified <see cref="IServiceProvider"/> instance.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsModel.</param>
        public EditModel(IServiceProvider serviceProvider)
        {
            _editHandler = new EditHandler<TUser, TRole>(serviceProvider, typeof(IndexModel));
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            RoleModel = new RoleModel();
            return await _editHandler.OnGetAsync(RoleModel, this, id);
        }

        public override async Task<IActionResult> OnPostAsync(string hfClaimList)
        {
            return await _editHandler.OnPostAsync(RoleModel, this, hfClaimList);
        }
    }
}
