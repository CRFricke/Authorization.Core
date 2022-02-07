using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Role
{
    [RequiresClaims(SysClaims.Role.Delete)]
    [PageImplementationType(typeof(DeleteModel<,>))]
    public abstract class DeleteModel : ModelBase
    {
        [BindProperty]
        public bool IsSystemRole { get; protected set; }

        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string id) => throw new NotImplementedException();
    }

    internal class DeleteModel<TUser, TRole> : DeleteModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<DeleteModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        public DeleteModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<DeleteModel> logger)
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

            RoleModel = await new RoleModel()
                .InitRoleClaims(_authManager)
                .InitFromRole(role)
                .InitRoleUsersAsync(_repository);

            return Page();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        public override async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _repository.Roles.FindAsync(id);
            if (role == null)
            {
                SendNotification(typeof(IndexModel), Severity.High,
                    $"Error: Role '{RoleModel.Name}' was not found in the database. Another user may have deleted it."
                    );

                return RedirectToPage(IndexModel.PageName);
            }

            await RoleModel.InitRoleUsersAsync(_repository);

            var result = await _authManager.AuthorizeAsync(User, role, new AppClaimRequirement(SysClaims.Role.Delete));
            if (!result.Succeeded)
            {
                var message = "System Roles may not be deleted.";
                ModelState.AddModelError(string.Empty, "Can not delete Role:");
                ModelState.AddModelError(string.Empty, message);

                _logger.LogError($"Could not delete {typeof(TRole).Name} '{role.Name}' (ID '{role.Id}'): {message}");

                return Page();
            }

            var userClaims = await (
                from uc in _repository.UserClaims
                join au in _repository.Users on uc.UserId equals au.Id
                where uc.ClaimType == ClaimTypes.Role && uc.ClaimValue == RoleModel.Name
                select uc
                ).ToArrayAsync();

            try
            {
                _repository.UserClaims.RemoveRange(userClaims);
                _repository.Roles.Remove(role);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not delete Role:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(ex, $"Could not delete {typeof(TRole).Name} '{role.Name}' (ID '{role.Id}').");

                return Page();
            }

            // Remove any users that were assigned this role from the UserClaim cache
            foreach (var claim in userClaims)
            {
                _authManager.RefreshUser(claim.UserId);
            }

            // Remove Role from RoleClaim cache
            _authManager.RefreshRole(role.Id);

            SendNotification(
                typeof(IndexModel), Severity.Normal,
                $"Role '{role.Name}' successfully deleted."
                );

            _logger.LogInformation($"{typeof(TRole).Name} '{role.Name}' (ID '{role.Id}') was deleted.");

            return RedirectToPage(IndexModel.PageName);
        }
    }
}
