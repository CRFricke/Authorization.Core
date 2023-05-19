using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.User;

internal class EditHandler<TUser, TRole>
    where TUser : AuthUiUser
    where TRole : AuthUiRole
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;
    private readonly ILogger<EditHandler> _logger;
    private readonly Type _notificationReceiver;

    /// <summary>
    /// Creates a new <see cref="EditHandler{TUser, TRole}"/> using the specified parameters. 
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{EditHandler}"/> instance to be used for logging.</param>
    /// <param name="notificationReceiver">The <see cref="Type"/> of the Razor page to receive notification messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the constructor's parameters are <see langword="null"/>.
    /// </exception>
    public EditHandler(
        IAuthorizationManager authManager,
        IRepository<TUser, TRole> repository,
        ILogger<EditHandler> logger,
        Type notificationReceiver)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationReceiver = notificationReceiver ?? throw new ArgumentNullException(nameof(notificationReceiver));
    }

    /// <summary>
    /// Called to initialize the <see cref="UserModel"/> for the Edit User page.
    /// </summary>
    /// <param name="userModel">The <see cref="UserModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Edit User page.</param>
    /// <param name="id">The ID (database key) of the user to be updated.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Edit User page.</returns>
    public async Task<IActionResult> OnGetAsync(UserModel userModel, ModelBase modelBase, string id)
    {
        if (id == null)
        {
            return modelBase.NotFound();
        }

        var user = await _repository.Users
            .Include(au => au.Claims)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (user == null)
        {
            return modelBase.NotFound();
        }

        userModel.IsSystemUser = _authManager.DefinedGuids.Contains(user.Id);

        (await userModel.InitRoleInfoAsync(_repository))
            .InitFromUser(user);

        return modelBase.Page();
    }

    /// <summary>
    /// Called to update the specified User database entity.
    /// </summary>
    /// <param name="userModel">A <see cref="UserModel"/> object that describes the User to be updated.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Edit User page.</param>
    /// <param name="hfRoleList">A list of Roles to be assigned to the User.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    public async Task<IActionResult> OnPostAsync(UserModel userModel, ModelBase modelBase, string hfRoleList)
    {
        (await userModel.InitRoleInfoAsync(_repository))
            .SetAssignedClaims(hfRoleList?.Split(',') ?? Array.Empty<string>());

        var modelState = modelBase.ModelState;
        var principal = modelBase.User;

        if (!modelState.IsValid)
        {
            return modelBase.Page();
        }

        var user = await _repository.Users
            .Include(au => au.Claims)
            .FirstOrDefaultAsync(m => m.Id == userModel.Id);

        if (user == null)
        {
            modelBase.SendNotification(
                _notificationReceiver, Severity.High,
                $"Error: User '{userModel.Email}' was not found in the database. Another user may have deleted it."
                );

            return modelBase.RedirectToPage(IndexHandler.PageName);
        }

        var rowsUpdated = 0;
        userModel.UpdateUser(user);

        if (userModel.ClaimsUpdated)
        {
            var result = await _authManager.AuthorizeAsync(principal, user, new AppClaimRequirement(SysClaims.User.UpdateClaims));
            if (!result.Succeeded)
            {
                modelState.AddModelError(string.Empty, "Can not update User:");

                if (result.Failure.FailureReason == AuthorizationFailure.Reason.SystemObject)
                {
                    modelState.AddModelError(string.Empty, "You may not update the Roles assigned to a system User.");

                    _logger.LogWarning(
                        "'{PrincipalEmail}' attempted to update the Roles of system {UserType} '{UserEmail}' (ID '{UserId}')",
                        principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                        );
                    return modelBase.Page();
                }

                if (principal.UserId() != user.Id)
                {
                    modelState.AddModelError(string.Empty, "You can not give a User more privileges than you have.");
                    _logger.LogWarning(
                        "'{PrincipalEmail}' attempted to give {UserType} '{UserEmail}' (ID '{UserId}') elevated privileges.",
                        principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                        );
                }
                else
                {
                    modelState.AddModelError(string.Empty, "You can not elevate your own privileges.");
                    _logger.LogWarning(
                        "'{PrincipalEmail}' attempted to elevate their own privileges.",
                        principal.Identity.Name
                        );
                }

                return modelBase.Page();
            }
        }

        try
        {
            rowsUpdated = await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not update User:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not update {UserType} '{UserEmail}' (ID '{UserId}').",
                principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );

            return modelBase.Page();
        }

        if (rowsUpdated > 0)
        {
            if (userModel.ClaimsUpdated)
            {
                _authManager.RefreshUser(user.Id);
            }

            modelBase.SendNotification(
                _notificationReceiver, Severity.Normal,
                $"User '{user.Email}' successfully updated."
                );

            _logger.LogInformation(
                "'{PrincipalEmail}' updated {UserType} '{UserEmail}' (ID '{UserId}').",
                principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );
        }

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class EditHandler { }
