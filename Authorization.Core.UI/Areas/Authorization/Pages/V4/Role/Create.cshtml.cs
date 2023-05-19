using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role
{
    [RequiresClaims(SysClaims.Role.Create)]
    [PageImplementationType(typeof(CreateModel<,>))]
    public abstract class CreateModel : ModelBase
    {
        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual IActionResult OnGet() => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfClaimList) => throw new NotImplementedException();
    }

    internal class CreateModel<TUser, TRole> : CreateModel
        where TRole : AuthUiRole, new()
        where TUser : AuthUiUser
    {
        private readonly CreateHandler<TUser, TRole> _createHandler;

        /// <summary>
        /// Creates a new <see cref="CreateModel{TUser, TRole}"/> class instance using the specified parameters.
        /// </summary>
        /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
        /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
        /// <param name="logger">The <see cref="ILogger{CreateHandler}"/> instance to be used for logging.</param>
        public CreateModel(
            IAuthorizationManager authManager,
            IRepository<TUser, TRole> repository,
            ILogger<CreateHandler> logger)
        {
            _createHandler = new CreateHandler<TUser, TRole>(authManager, repository, logger, typeof(IndexModel));
        }

        public override IActionResult OnGet()
        {
            RoleModel = new RoleModel();
            return _createHandler.OnGet(RoleModel, this);
        }

        public override async Task<IActionResult> OnPostAsync(string hfClaimList)
        {
            return await _createHandler.OnPostAsync(RoleModel, this, hfClaimList);
        }
    }
}
