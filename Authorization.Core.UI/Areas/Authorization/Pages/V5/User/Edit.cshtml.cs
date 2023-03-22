using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User
{
    [RequiresClaims(SysClaims.User.Update)]
    [PageImplementationType(typeof(EditModel<,>))]
    public abstract class EditModel : ModelBase
    {
        public bool IsSystemUser { get; protected set; }

        [BindProperty]
        public UserModel Input { get; set; }

        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
    }

    internal class EditModel<TUser, TRole> : EditModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<EditModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new EditModel<TUser> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the EditModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the EditModel.</param>
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

            var user = await _repository.Users
                .Include(au => au.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            IsSystemUser = _authManager.DefinedGuids.Contains(user.Id);

            Input = (await new UserModel()
                .InitRoleInfoAsync(_repository))
                .InitFromUser(user);

            return Page();
        }

        public override async Task<IActionResult> OnPostAsync(string hfRoleList)
        {
            (await Input.InitRoleInfoAsync(_repository))
                .SetAssignedClaims(hfRoleList?.Split(',') ?? Array.Empty<string>());

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _repository.Users
                .Include(au => au.Claims)
                .FirstOrDefaultAsync(m => m.Id == Input.Id);

            if (user == null)
            {
                SendNotification(typeof(IndexModel), Severity.High,
                    $"Error: User '{Input.Email}' was not found in the database. Another user may have deleted it."
                    );

                return RedirectToPage(IndexModel.PageName);
            }

            var rowsUpdated = 0;
            Input.UpdateUser(user);

            if (Input.ClaimsUpdated)
            {
                var result = await _authManager.AuthorizeAsync(User, user, new AppClaimRequirement(SysClaims.User.UpdateClaims));
                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Can not update User:");

                    if (result.Failure.FailureReason == AuthorizationFailure.Reason.SystemObject)
                    {
                        ModelState.AddModelError(string.Empty, "You may not update the Roles assigned to a system User.");
                        _logger.LogWarning(
                            "'{PrincipalEmail}' attempted to update the Roles of system {UserType} '{UserEmail}' (ID '{UserId}')",
                            User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                            );

                        return Page();
                    }

                    if (User.UserId() != user.Id)
                    {
                        ModelState.AddModelError(string.Empty, "You can not give a User more privileges than you have.");
                        _logger.LogWarning(
                            "'{PrincipalEmail}' attempted to give {UserType} '{UserEmail}' (ID '{UserId}') elevated privileges.",
                            User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                            );
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "You can not elevate your own privileges.");
                        _logger.LogWarning(
                            "'{PrincipalEmail}' attempted to elevate their own privileges.",
                            User.Identity.Name
                            );
                    }

                    return Page();
                }
            }

            try
            {
                rowsUpdated = await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not update User:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(
                    ex, "'{PrincipalEmail}' could not update {UserType} '{UserEmail}' (ID '{UserId}').",
                    User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                    );

                return Page();
            }

            if (rowsUpdated > 0)
            {
                if (Input.ClaimsUpdated)
                {
                    _authManager.RefreshUser(user.Id);
                }

                SendNotification(
                    typeof(IndexModel), Severity.Normal,
                    $"User '{user.Email}' successfully updated."
                    );

                _logger.LogInformation(
                    "'{PrincipalEmail}' updated {UserType} '{UserEmail}' (ID '{UserId}').",
                    User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                    );
            }

            return RedirectToPage(IndexModel.PageName);
        }
    }
}
