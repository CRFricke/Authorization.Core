using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.User
{
    [RequiresClaims(SysClaims.User.Create)]
    [PageImplementationType(typeof(CreateModel<,>))]
    public abstract class CreateModel : ModelBase
    {
        [BindProperty]
        public UserModel UserModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync() => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
    }

    internal class CreateModel<TUser, TRole> : CreateModel
        where TUser : AuthUiUser, new()
        where TRole : AuthUiRole
    {
        private readonly CreateHandler<TUser, TRole> _createHandler;

        /// <summary>
        /// Creates a new <see cref="CreateModel{TUser, TRole}"/> class instance using the specified parameters.
        /// </summary>
        /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
        /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
        /// <param name="logger">The <see cref="ILogger{CreateHandler}"/> instance to be used for logging.</param>
        /// <param name="passwordHasher">The <see cref="IPasswordHasher{TUser}"/> instance to be used to hash the supplied password.</param>
        public CreateModel(
            IAuthorizationManager authManager,
            IRepository<TUser, TRole> repository,
            ILogger<CreateHandler> logger,
            IPasswordHasher<TUser> passwordHasher)
        {
            _createHandler = new CreateHandler<TUser, TRole>(authManager, repository, logger, passwordHasher, typeof(IndexModel));
        }

        public override async Task<IActionResult> OnGetAsync()
        {
            UserModel = new UserModel();
            return await _createHandler.OnGetAsync(UserModel, this);
        }

        public override async Task<IActionResult> OnPostAsync(string hfRoleList)
        {
            return await _createHandler.OnPostAsync(UserModel, this, hfRoleList);
        }
    }
}
