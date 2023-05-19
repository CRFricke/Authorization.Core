using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.User
{
    [RequiresClaims(SysClaims.User.Update)]
    [PageImplementationType(typeof(EditModel<,>))]
    public abstract class EditModel : ModelBase
    {
        [BindProperty]
        public UserModel UserModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
    }

    internal class EditModel<TUser, TRole> : EditModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly EditHandler<TUser, TRole> _editHandler;

        /// <summary>
        /// Creates a new <see cref="EditModel{TUser, TRole}"/> class instance using the specified parameters.
        /// </summary>
        /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
        /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
        /// <param name="logger">The <see cref="ILogger{EditHandler}"/> instance to be used for logging.</param>
        public EditModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<EditHandler> logger)
        {
            _editHandler = new EditHandler<TUser, TRole>(authManager, repository, logger, typeof(IndexModel));
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            UserModel = new();
            return await _editHandler.OnGetAsync(UserModel, this, id);
        }

        public override async Task<IActionResult> OnPostAsync(string hfRoleList)
        {
            return await _editHandler.OnPostAsync(UserModel, this, hfRoleList);
        }
    }
}
