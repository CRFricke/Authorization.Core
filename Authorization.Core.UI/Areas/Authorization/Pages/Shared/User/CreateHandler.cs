using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.User;

internal class CreateHandler<TUser, TRole>
    where TUser : AuthUiUser, new()
    where TRole : AuthUiRole
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;
    private readonly ILogger<CreateHandler> _logger;
    private readonly IPasswordHasher<TUser> _passwordHasher;
    private readonly Type _notificationReceiver;

    /// <summary>
    /// Creates a new <see cref="CreateHandler{TUser, TRole}"/> using the specified parameters. 
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{CreateHandler}"/> instance to be used for logging.</param>
    /// <param name="passwordHasher">The <see cref="IPasswordHasher{TUser}"/> instance to be used to hash the supplied password.</param>
    /// <param name="notificationReceiver">The <see cref="Type"/> of the Razor page to receive notification messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the constructor's parameters are <see langword="null"/>.
    /// </exception>
    public CreateHandler(
        IAuthorizationManager authManager, 
        IRepository<TUser, TRole> repository, 
        ILogger<CreateHandler> logger, 
        IPasswordHasher<TUser> passwordHasher, 
        Type notificationReceiver)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _notificationReceiver = notificationReceiver ?? throw new ArgumentNullException(nameof(notificationReceiver));
    }


    /// <summary>
    /// Called to initialize the <see cref="UserModel"/> for the Create User page.
    /// </summary>
    /// <param name="userModel">The <see cref="UserModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Create User page.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Create User page.</returns>
    public async Task<IActionResult> OnGetAsync(UserModel userModel, ModelBase modelBase)
    {
        await userModel.InitRoleInfoAsync(_repository);
        return modelBase.Page();
    }

    /// <summary>
    /// Called to create the new User database entity.
    /// </summary>
    /// <param name="userModel">The <see cref="UserModel"/> object to be used to initialize the new User.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Create User page.</param>
    /// <param name="hfRoleList">A list of Roles to be assigned to the new User.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    public async Task<IActionResult> OnPostAsync(UserModel userModel, ModelBase modelBase, string hfRoleList)
    {
        var modelState = modelBase.ModelState;
        var principal = modelBase.User;

        (await userModel.InitRoleInfoAsync(_repository))
            .SetAssignedClaims(hfRoleList?.Split(',') ?? Array.Empty<string>());

        if (!modelState.IsValid)
        {
            return modelBase.Page();
        }

        var user = new TUser();
        userModel.UpdateUser(user);

        var result = await _authManager.AuthorizeAsync(principal, user, new AppClaimRequirement(SysClaims.User.Create));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not create User:");
            modelState.AddModelError(string.Empty, "You can not create a User with more privileges than you have.");

            _logger.LogWarning(
                "'{PrincipalEmail}' attempted to create {UserType} with elevated privileges.",
                principal.Identity.Name, typeof(TUser).Name
                );

            return modelBase.Page();
        }

        _passwordHasher.HashPassword(user, userModel.Password);

        try
        {
            _repository.Users.Add(user);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not create User:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not create {UserType} '{UserEmail}' (ID '{UserId}').",
                principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );

            return modelBase.Page();
        }

        modelBase.SendNotification(
            _notificationReceiver, Severity.Normal,
            $"User '{user.Email}' successfully created."
            );

        _logger.LogInformation(
            "'{PrincipalEmail}' created {UserType} '{UserEmail}' (ID '{UserId}').",
            principal.Identity.Name, typeof(TUser).Name, user.Email, user.Id
            );

        return modelBase.RedirectToPage(IndexHandler.PageName);
    }
}

/// <summary>
/// For logging only.
/// </summary>
class CreateHandler { }

