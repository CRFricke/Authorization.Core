using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;
    private readonly ILogger<DeleteHandler> _logger;
    private readonly Type _notificationReceiver;

    /// <summary>
    /// Creates a new <see cref="DeleteHandler{TUser, TRole}"/> using the specified parameters. 
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{DeleteHandler}"/> instance to be used for logging.</param>
    /// <param name="notificationReceiver">The <see cref="Type"/> of the Razor page to receive notification messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the constructor's parameters are <see langword="null"/>.
    /// </exception>
    public DeleteHandler(
        IAuthorizationManager authManager,
        IRepository<TUser, TRole> repository,
        ILogger<DeleteHandler> logger,
        Type notificationReceiver)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationReceiver = notificationReceiver ?? throw new ArgumentNullException(nameof(notificationReceiver));
    }

    /// <summary>
    /// Called to initialize the <see cref="RoleModel"/> for the Delete Role page.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Delete Role page.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Delete Role page.</returns>
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

    /// <summary>
    /// Called to delete the specified Role from the database.
    /// </summary>
    /// <param name="roleModel">A <see cref="RoleModel"/> object that describes the Role to be deleted.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Delete Role page.</param>
    /// <param name="id">The key (database ID) of the User to be deleted.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    public async Task<IActionResult> OnPostAsync(RoleModel roleModel, ModelBase modelBase, string id)
    {
        if (id == null)
        {
            return modelBase.NotFound();
        }

        var modelState = modelBase.ModelState;
        var principal = modelBase.User;

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

        var result = await _authManager.AuthorizeAsync(principal, role, new AppClaimRequirement(SysClaims.Role.Delete));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not delete Role:");
            modelState.AddModelError(string.Empty, "System Roles may not be deleted.");

            _logger.LogWarning(
                "'{PrincipalEmail}' attempted to delete system {RoleType} '{RoleName}' (ID: {RoleId}).",
                principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
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

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not delete {RoleType} '{RoleName}' (ID: {RoleId}).",
                principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
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

        _logger.LogInformation(
            "'{PrincipalEmail}' deleted {RoleType} '{RoleName}' (ID: {RoleId}).",
            principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
            );

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class DeleteHandler { }
