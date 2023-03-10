using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
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

    internal class DetailsModel<
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser, 
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
        > : DetailsModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new DetailsModel<TRole> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the DetailsModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the DetailsModel.</param>
        public DetailsModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository)
        {
            _authManager = authManager;
            _repository = repository;
        }

        public override async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _repository.Roles
                .Include(ar => ar.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            RoleModel = new RoleModel()
                .InitRoleClaims(_authManager)
                .InitFromRole(role);

            return Page();
        }
    }
}
