using Fricke.Authorization.Core.Attributes;
using Fricke.Authorization.Core.UI.Data;
using Fricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Fricke.Authorization.Core.UI.Pages.Role
{
    [RequiresClaim(SysClaims.Role.Read)]
    [PageImplementationType(typeof(DetailsModel<,>))]
    public abstract class DetailsModel : ModelBase
    {
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id)
            => throw new NotImplementedException();
    }

    internal class DetailsModel<TUser, TRole> : DetailsModel
        where TUser : AppUser
        where TRole : AppRole
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
