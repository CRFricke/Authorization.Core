using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.User;

internal class DeleteHandler<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole>
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
    /// Called to initialize the <see cref="UserModel"/> for the Delete User page.
    /// </summary>
    /// <param name="userModel">The <see cref="UserModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Delete User page.</param>
    /// <param name="id">The ID (database key) of the user to be deleted.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Delete User page.</returns>
    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
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
    /// Called to delete the specified User database entity.
    /// </summary>
    /// <param name="userModel">A <see cref="UserModel"/> object that describes the User to be deleted.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Delete User page.</param>
    /// <param name="id">The ID (database key) of the user to be deleted.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public async Task<IActionResult> OnPostAsync(UserModel userModel, ModelBase modelBase, string id)
    {
        if (id == null)
        {
            return modelBase.NotFound();
        }

        var user = await _repository.Users
            .Include(au => au.Claims)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (user == null)
        {
            modelBase.SendNotification(
                _notificationReceiver, Severity.High,
                $"Error: User '{userModel.Email}' was not found in the database. Another user may have deleted it."
                );

            return modelBase.RedirectToPage(IndexHandler.PageName);
        }

        var modelState = modelBase.ModelState;
        var principal = modelBase.User;

        // Don't care about ModelState on Delete.
        modelState.Clear();

        var result = await _authManager.AuthorizeAsync(principal, user, new AppClaimRequirement(SysClaims.User.Delete));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not delete User:");
            modelState.AddModelError(string.Empty, "System accounts may not be deleted.");

            _logger.LogWarning(
                "'{PrincipalEmail}' attempted to delete system {UserType} '{UserEmail}' (ID '{UserId}').",
                principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );

            (await userModel.InitRoleInfoAsync(_repository)).InitFromUser(user);
            return modelBase.Page();
        }

        try
        {
            _repository.Users.Remove(user);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not delete User:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not delete {UserType} '{UserEmail}' (ID '{UserId}').",
                principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );
        }

        if (!modelState.IsValid)
        {
            (await userModel.InitRoleInfoAsync(_repository)).InitFromUser(user);
            return modelBase.Page();
        }

        // Remove User from UserClaims cache
        _authManager.RefreshUser(user.Id);

        modelBase.SendNotification(
            _notificationReceiver, Severity.Normal,
            $"User '{user.Email}' successfully deleted."
            );

        _logger.LogInformation(
            "'{PrincipalEmail}' deleted {UserType} '{UserEmail}' (ID '{UserId}').",
            principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
            );

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class DeleteHandler { }
