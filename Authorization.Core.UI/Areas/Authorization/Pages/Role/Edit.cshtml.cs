using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Role
{
    [RequiresClaims(SysClaims.Role.Update)]
    [PageImplementationType(typeof(EditModel<,>))]
    public abstract class EditModel : ModelBase
    {
        public bool IsSystemRole { get; protected set; }

        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfClaimList) => throw new NotImplementedException();
    }

    internal class EditModel<TUser, TRole> : EditModel
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new EditModel<TUser, TRole> class instance using the specified <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository">The repository instance to be used to initialize the IndexModel.</param>
        public EditModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<EditModel> logger)
        {
            _authManager = authManager;
            _logger = logger;
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

            IsSystemRole = _authManager.DefinedGuids.Contains(role.Id);

            RoleModel = new RoleModel()
                .InitRoleClaims(_authManager)
                .InitFromRole(role);

            return Page();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        public override async Task<IActionResult> OnPostAsync(string hfClaimList)
        {
            RoleModel.InitRoleClaims(_authManager)
                .SetAssignedClaims(
                    hfClaimList?.Split(',') ?? Array.Empty<string>()
                    );

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var role = await _repository.Roles
                .Include(ar => ar.Claims)
                .FirstOrDefaultAsync(m => m.Id == RoleModel.Id);

            if (role == null)
            {
                SendNotification(typeof(IndexModel), Severity.High,
                    $"Error: Role '{RoleModel.Name}' was not found in the database. Another user may have deleted it."
                    );

                return RedirectToPage(IndexModel.PageName);
            }

            var rowsUpdated = 0;
            RoleModel.UpdateRole(role);

            if (RoleModel.ClaimsUpdated)
            {
                var result = await _authManager.AuthorizeAsync(User, role, new AppClaimRequirement(SysClaims.Role.UpdateClaims));
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Can not update Role:");

                    if (result.Failure.FailureReason == AuthorizationFailure.Reason.SystemObject)
                    {
                        var message = "You may not update the Claims assigned to a system Role.";
                        ModelState.AddModelError(string.Empty, message);

                        _logger.LogInformation($"Could not update {typeof(TRole).Name} '{role.Name}' (ID '{role.Id}'): {message}");
                        return Page();
                    }

                    ModelState.AddModelError(string.Empty, "You can not give a Role more privileges than you have.");

                    _logger.LogInformation($"User '{User.Identity.Name}' (ID '{User.UserId()}') attempted to give {typeof(TRole).Name} elevated privileges.");
                    return Page();
                }
            }

            try
            {
                rowsUpdated = await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not update Role:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(ex, $"Could not update {typeof(TRole).Name} '{role.Name}' (ID '{role.Id}').");

                return Page();
            }

            if (rowsUpdated > 0)
            {
                if (RoleModel.ClaimsUpdated)
                {
                    _authManager.RefreshRole(role.Id);
                }

                SendNotification(
                    typeof(IndexModel), Severity.Normal,
                    $"Role '{role.Name}' was successfully updated."
                    );

                _logger.LogInformation($"{typeof(TRole).Name} '{role.Name}' (ID '{role.Id}') was updated.");
            }

            return RedirectToPage(IndexModel.PageName);
        }
    }
}
