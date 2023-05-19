using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class CreateHandler<TUser, TRole>
    where TRole : AuthUiRole, new()
    where TUser : AuthUiUser
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;
    private readonly ILogger<CreateHandler> _logger;
    private readonly Type _notificationReceiver;

    /// <summary>
    /// Creates a new <see cref="CreateHandler{TUser, TRole}"/> using the specified parameters. 
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{CreateHandler}"/> instance to be used for logging.</param>
    /// <param name="notificationReceiver">The <see cref="Type"/> of the Razor page to receive notification messages.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the constructor's parameters are <see langword="null"/>.
    /// </exception>
    public CreateHandler(
        IAuthorizationManager authManager,
        IRepository<TUser, TRole> repository,
        ILogger<CreateHandler> logger,
        Type notificationReceiver)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationReceiver = notificationReceiver ?? throw new ArgumentNullException(nameof(notificationReceiver));
    }


    /// <summary>
    /// Called to initialize the <see cref="RoleModel"/> for the Create Role page.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Create Role page.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Create Role page.</returns>
    public IActionResult OnGet(RoleModel roleModel, ModelBase modelBase)
    {
        roleModel.InitRoleClaims(_authManager);
        return modelBase.Page();
    }

    /// <summary>
    /// Called to create the new Role database entity.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> object to be used to initialize the new Role.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Create Role page.</param>
    /// <param name="hfClaimList">A list of Claims to be assigned to the new Role.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the next Razor page.</returns>
    /// <exception cref="InvalidOperationException">
    /// <list type="bullet">
    /// <item>Thrown if no <see cref="ILoggerFactory"/> implementation can be found by the the <see cref="IServiceProvider"/>.</item>
    /// <item>Thrown if no <see cref="IRepository{TUser, TRole}"/> implementation can be found by the the <see cref="IServiceProvider"/>.</item>
    /// </list>
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

        var role = CreateRole(roleModel);

        var result = await _authManager.AuthorizeAsync(principal, role, new AppClaimRequirement(SysClaims.Role.Create));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not create Role:");
            modelState.AddModelError(string.Empty, "You can not create a Role with more privileges than you have.");

            _logger.LogWarning(
                "'{PrincipalEmail}' attempted to create {RoleType} with elevated privileges.",
                principal.Identity.Name, typeof(TRole).Name
                );

            return modelBase.Page();
        }

        try
        {
            _repository.Roles.Add(role);
            await _repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not create Role:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            _logger.LogError(
                ex, "'{PrincipalEmail}' could not create {RoleType} '{RoleName}' (ID: {RoleId}).",
                principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );

            return modelBase.Page();
        }

        modelBase.SendNotification(
            _notificationReceiver, Severity.Normal,
            $"Role '{role.Name}' successfully created."
            );

        _logger.LogInformation(
            "'{PrincipalEmail}' created {RoleType} '{RoleName}' (ID: {RoleId}).",
            principal.Identity.Name, typeof(TRole).Name, role.Name, role.Id
            );

        return modelBase.RedirectToPage(IndexHandler.PageName);

    }

    private static TRole CreateRole(RoleModel model)
    {
        var normalizer = new UpperInvariantLookupNormalizer();

        var role = new TRole
        {
            Description = model.Description,
            Name = model.Name,
            NormalizedName = normalizer.NormalizeName(model.Name)
        }.SetClaims(model.GetAssignedClaims());

        return role;
    }
}

/// <summary>
/// For logging only.
/// </summary>
class CreateHandler { }
