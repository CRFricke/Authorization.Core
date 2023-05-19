using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.User
{
    [RequiresClaims(SysClaims.User.Read)]
    [PageImplementationType(typeof(DetailsModel<,>))]
    public abstract class DetailsModel : ModelBase
    {
        public UserModel UserModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();
    }

    internal class DetailsModel<TUser, TRole> : DetailsModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly DetailsHandler<TUser, TRole> _detailsHandler;

        /// <summary>
        /// Creates a new <see cref="DetailsModel{TUser, TRole}"/> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the DetailsModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the DetailsModel.</param>
        public DetailsModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository)
        {
            _detailsHandler = new DetailsHandler<TUser, TRole>(authManager, repository);
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            UserModel = new();
            return await _detailsHandler.OnGetAsync(UserModel, this, id);
        }
    }
}
