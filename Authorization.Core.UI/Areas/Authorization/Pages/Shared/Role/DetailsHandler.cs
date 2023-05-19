using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    /// Creates a new <see cref="DetailsHandler{TUser, TRole}"/> class instance using the specified parameters.
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the constructor's parameters are <see langword="null"/>.
    /// </exception>
    public DetailsHandler(IAuthorizationManager authManager, IRepository<TUser, TRole> repository)
    {
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Called to initialize the <see cref="RoleModel"/> for the Role Details page.
    /// </summary>
    /// <param name="roleModel">The <see cref="RoleModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the Role Details page.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Role Details page.</returns>
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
