using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class EditHandler<TUser, TRole>
    where TRole : AuthUiRole
    where TUser : AuthUiUser
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _notificationReceiver;
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;

    public EditHandler(IServiceProvider serviceProvider, Type notificationReceiver)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _notificationReceiver = notificationReceiver ?? throw new ArgumentNullException(nameof(notificationReceiver));
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _repository = serviceProvider.GetRequiredService<IRepository<TUser, TRole>>();
    }

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

    public async Task<IActionResult> OnPostAsync(RoleModel roleModel, ModelBase modelBase, string hfClaimList)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<EditHandler>();
        var modelState = modelBase.ModelState;
        var user = modelBase.User;

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
            var result = await _authManager.AuthorizeAsync(user, role, new AppClaimRequirement(SysClaims.Role.UpdateClaims));
            if (!result.Succeeded)
            {
                modelState.AddModelError(string.Empty, "Can not update Role:");

                if (result.Failure.FailureReason == AuthorizationFailure.Reason.SystemObject)
                {
                    var message = "You may not update the Claims assigned to a system Role.";
                    modelState.AddModelError(string.Empty, message);
                    logger.LogWarning(
                        "'{PrincipalEmail}' attempted to update the claims of system {RoleType} '{RoleName}' (ID: {RoleId}).",
                        user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                        );
                    return modelBase.Page();
                }

                modelState.AddModelError(string.Empty, "You can not give a Role more privileges than you have.");
                logger.LogWarning(
                    "'{PrincipalEmail}' attempted to give {RoleType} '{RoleName}' (ID: {RoleId}) elevated privileges.",
                    user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
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

            logger.LogError(
                ex, "'{PrincipalEmail}' could not update {RoleType} '{RoleName}' (ID: {RoleId}).",
                user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
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

            logger.LogInformation(
                "'{PrincipalEmail}' updated {RoleType} '{RoleName}' (ID: {RoleId}).",
                user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );
        }

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class EditHandler { }
