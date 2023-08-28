using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class EditHandler<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole>
    where TRole : AuthUiRole
    where TUser : AuthUiUser
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;
    private readonly ILogger<EditHandler> _logger;
    private readonly Type _notificationReceiver;

    /// <summary>
    /// Creates a new <see cref="EditHandler{TUser, TRole}"/> class instance using the specified parameters.
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
    /// Called to initialize the <see cref="RoleModel"/> for the Edit Role page.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Edit Role page.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Edit Role page.</returns>
    public async Task<IActionResult> OnGetAsync(RoleModel roleModel, ModelBase modelBase, string id)
    {
        if (id == null)
        {
            return modelBase.NotFound();
        }

        var role = await _repository.Roles
            .Include(ar => ar.Claims)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (role == null)
        {
            return modelBase.NotFound();
        }

        roleModel.IsSystemRole = _authManager.DefinedGuids.Contains(role.Id);

        roleModel
            .InitRoleClaims(_authManager)
            .InitFromRole(role);

        return modelBase.Page();
    }

    /// <summary>
    /// Called to update the Role in the database.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> object to be used to update the Role.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Edit Role page.</param>
    /// <param name="hfClaimList">A list of Claims to be assigned to the Role.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no <see cref="ILoggerFactory"/> implementation can be found by the the <see cref="IServiceProvider"/>.
    /// </exception>
    public async Task<IActionResult> OnPostAsync(RoleModel roleModel, ModelBase modelBase, string hfClaimList)
    {
        var modelState = modelBase.ModelState;
        var principal = modelBase.User;

        roleModel.InitRoleClaims(_authManager)
            .SetAssignedClaims(
                hfClaimList?.Split(',') ?? Array.Empty<string>()
                );

        if (!modelState.IsValid)
        {
            return modelBase.Page();
        }

        var role = await _repository.Roles
            .Include(ar => ar.Claims)
            .FirstOrDefaultAsync(m => m.Id == roleModel.Id);

        if (role == null)
        {
            modelBase.SendNotification(
                _notificationReceiver, Severity.High,
                $"Error: Role '{roleModel.Name}' was not found in the database. Another user may have deleted it."
                );

            return modelBase.RedirectToPage(IndexHandler.PageName);
        }

        var rowsUpdated = 0;
        roleModel.UpdateRole(role);

        if (roleModel.ClaimsUpdated)
        {
            var result = await _authManager.AuthorizeAsync(principal, role, new AppClaimRequirement(SysClaims.Role.UpdateClaims));
            if (!result.Succeeded)
            {
                modelState.AddModelError(string.Empty, "Can not update Role:");

                if (result.Failure.FailureReason == AuthorizationFailure.Reason.SystemObject)
                {
                    var message = "You may not update the Claims assigned to a system Role.";
                    modelState.AddModelError(string.Empty, message);
                    _logger.LogWarning(
                        "'{PrincipalEmail}' attempted to update the claims of system {RoleType} '{RoleName}' (ID: {RoleId}).",
                        principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                        );
                    return modelBase.Page();
                }

                modelState.AddModelError(string.Empty, "You can not give a Role more privileges than you have.");
                _logger.LogWarning(
                    "'{PrincipalEmail}' attempted to give {RoleType} '{RoleName}' (ID: {RoleId}) elevated privileges.",
                    principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                    );
                return modelBase.Page();
            }
        }

        try
        {
            rowsUpdated = await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not update Role:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not update {RoleType} '{RoleName}' (ID: {RoleId}).",
                principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );

            return modelBase.Page();
        }

        if (rowsUpdated > 0)
        {
            if (roleModel.ClaimsUpdated)
            {
                _authManager.RefreshRole(role.Id);
            }

            modelBase.SendNotification(
                _notificationReceiver, Severity.Normal,
                $"Role '{role.Name}' was successfully updated."
                );

            _logger.LogInformation(
                "'{PrincipalEmail}' updated {RoleType} '{RoleName}' (ID: {RoleId}).",
                principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );
        }

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class EditHandler { }
