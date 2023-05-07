using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class DeleteHandler<TUser, TRole>
        where TUser : AuthUiUser
        where TRole : AuthUiRole
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _notificationReceiver;
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;

    public DeleteHandler(IServiceProvider serviceProvider, Type notificationReceiver)
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

        await roleModel
            .InitRoleClaims(_authManager)
            .InitFromRole(role)
            .InitRoleUsersAsync(_repository);

        return modelBase.Page();
    }

    public async Task<IActionResult> OnPostAsync(RoleModel roleModel, ModelBase modelBase, string id)
    {
        if (id == null)
        {
            return modelBase.NotFound();
        }

        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<DeleteHandler>();
        var modelState = modelBase.ModelState;
        var user = modelBase.User;

        var role = await _repository.Roles.FindAsync(id);
        if (role == null)
        {
            modelBase.SendNotification(
                _notificationReceiver, Severity.High,
                $"Error: Role '{roleModel.Name}' was not found in the database. Another user may have deleted it."
                );

            return modelBase.RedirectToPage(IndexHandler.PageName);
        }

        // Don't care about ModelState on Delete.
        modelState.Clear();

        var result = await _authManager.AuthorizeAsync(user, role, new AppClaimRequirement(SysClaims.Role.Delete));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not delete Role:");
            modelState.AddModelError(string.Empty, "System Roles may not be deleted.");

            logger.LogWarning(
                "'{PrincipalEmail}' attempted to delete system {RoleType} '{RoleName}' (ID: {RoleId}).",
                user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );

            await roleModel.InitRoleClaims(_authManager)
                .InitFromRole(role)
                .InitRoleUsersAsync(_repository);

            return modelBase.Page();
        }

        var userClaims = await (
            from uc in _repository.UserClaims
            join au in _repository.Users on uc.UserId equals au.Id
            where uc.ClaimType == ClaimTypes.Role && uc.ClaimValue == role.Name
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
            modelState.AddModelError(string.Empty, "Could not delete Role:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            logger.LogError(
                ex, "'{PrincipalEmail}' could not delete {RoleType} '{RoleName}' (ID: {RoleId}).",
                user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );

            await roleModel.InitRoleClaims(_authManager)
                .InitFromRole(role)
                .InitRoleUsersAsync(_repository);

            return modelBase.Page();
        }

        // Remove any users that were assigned this role from the UserClaim cache
        foreach (var claim in userClaims)
        {
            _authManager.RefreshUser(claim.UserId);
        }

        // Remove Role from RoleClaim cache
        _authManager.RefreshRole(role.Id);

        modelBase.SendNotification(
            _notificationReceiver, Severity.Normal,
            $"Role '{role.Name}' successfully deleted."
            );

        logger.LogInformation(
            "'{PrincipalEmail}' deleted {RoleType} '{RoleName}' (ID: {RoleId}).",
            user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
            );

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class DeleteHandler { }
