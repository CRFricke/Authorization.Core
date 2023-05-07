using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.Role
{
    [RequiresClaims(SysClaims.Role.Delete)]
    [PageImplementationType(typeof(DeleteModel<,>))]
    public abstract class DeleteModel : ModelBase
    {
        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string id) => throw new NotImplementedException();
    }

    internal class DeleteModel<TUser, TRole> : DeleteModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly DeleteHandler<TUser, TRole> _deleteHandler;

        /// <summary>
        /// Creates a new DeleteModel&lt;TUser, TRole&gt; class instance using the specified <see cref="IServiceProvider"/> instance.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsModel.</param>
        public DeleteModel(IServiceProvider serviceProvider)
        {
            _deleteHandler = new DeleteHandler<TUser, TRole>(serviceProvider, typeof(IndexModel));
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            RoleModel = new RoleModel();
            return await _deleteHandler.OnGetAsync(RoleModel, this, id);
        }

        public override async Task<IActionResult> OnPostAsync(string id)
        {
            return await _deleteHandler.OnPostAsync(RoleModel, this, id);
        }
    }
}
