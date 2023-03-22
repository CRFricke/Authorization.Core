using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User
{
    [RequiresClaims(SysClaims.User.Read)]
    [PageImplementationType(typeof(DetailsModel<,>))]
    public abstract class DetailsModel : ModelBase
    {
        public bool IsSystemUser { get; protected set; }

        public UserModel UserModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();
    }

    internal class DetailsModel<TUser, TRole> : DetailsModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new DetailsModel<TUser, TRole> class instance using the specified authorization manager and repository.
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

            var user = await _repository.Users
                .Include(au => au.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            IsSystemUser = _authManager.DefinedGuids.Contains(user.Id);

            UserModel = (await new UserModel()
                .InitRoleInfoAsync(_repository))
                .InitFromUser(user);

            return Page();
        }
    }
}
