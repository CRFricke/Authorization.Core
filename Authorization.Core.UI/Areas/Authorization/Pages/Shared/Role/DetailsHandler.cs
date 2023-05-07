using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class DetailsHandler<TUser, TRole>
        where TUser : AuthUiUser
        where TRole : AuthUiRole
{
    private readonly IAuthorizationManager _authManager;
    private readonly IRepository<TUser, TRole> _repository;

    /// <summary>
    /// Creates a new DetailsHandler class instance using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsHandler.</param>
    public DetailsHandler(IServiceProvider serviceProvider)
    {
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

        roleModel
            .InitRoleClaims(_authManager)
            .InitFromRole(role);

        return modelBase.Page();
    }
}
