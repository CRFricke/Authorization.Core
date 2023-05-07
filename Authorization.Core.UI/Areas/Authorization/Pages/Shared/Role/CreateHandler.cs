using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class CreateHandler<TUser, TRole>
    where TRole : AuthUiRole, new()
    where TUser : AuthUiUser
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _notificationReceiver;
    private readonly IAuthorizationManager _authManager;

    public CreateHandler(IServiceProvider serviceProvider, Type notificationReceiver)
    {
        _serviceProvider = serviceProvider;
        _notificationReceiver = notificationReceiver;
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
    }


    public IActionResult OnGet(RoleModel roleModel, ModelBase modelBase)
    {
        roleModel.InitRoleClaims(_authManager);
        return modelBase.Page();
    }

    public async Task<IActionResult> OnPostAsync(RoleModel roleModel, ModelBase modelBase, string hfClaimList)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<CreateHandler>();
        var repository = _serviceProvider.GetRequiredService<IRepository<TUser, TRole>>();
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

        var role = CreateRole(roleModel);

        var result = await _authManager.AuthorizeAsync(user, role, new AppClaimRequirement(SysClaims.Role.Create));
        if (!result.Succeeded)
        {
            modelState.AddModelError(string.Empty, "Can not create Role:");
            modelState.AddModelError(string.Empty, "You can not create a Role with more privileges than you have.");

            logger.LogWarning(
                "'{PrincipalEmail}' attempted to create {RoleType} with elevated privileges.",
                user.Identity.Name, typeof(TRole).Name
                );

            return modelBase.Page();
        }

        try
        {
            repository.Roles.Add(role);
            await repository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            modelState.AddModelError(string.Empty, "Could not create Role:");
            modelState.AddModelError(string.Empty, ex.GetBaseException().Message);

            logger.LogError(
                ex, "'{PrincipalEmail}' could not create {RoleType} '{RoleName}' (ID: {RoleId}).",
                user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
                );

            return modelBase.Page();
        }

        modelBase.SendNotification(
            _notificationReceiver, Severity.Normal,
            $"Role '{role.Name}' successfully created."
            );

        logger.LogInformation(
            "'{PrincipalEmail}' created {RoleType} '{RoleName}' (ID: {RoleId}).",
            user.Identity.Name, typeof(TRole).Name, role.Name, role.Id
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
