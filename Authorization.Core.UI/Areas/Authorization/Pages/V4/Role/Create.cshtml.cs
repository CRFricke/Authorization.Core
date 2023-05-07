using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
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
        /// Creates a new CreateModel&lt;TUser, TRole&gt; class instance using the specified <see cref="IServiceProvider"/> instance.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsModel.</param>
        public CreateModel(IServiceProvider serviceProvider)
        {
            _createHandler = new CreateHandler<TUser, TRole>(serviceProvider, typeof(IndexModel));
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
