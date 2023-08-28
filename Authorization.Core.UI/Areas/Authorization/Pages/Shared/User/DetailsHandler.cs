using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.User;

internal class DetailsHandler<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser, 
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole>
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
    /// Called to initialize the <see cref="UserModel"/> for the User Details page.
    /// </summary>
    /// <param name="userModel">The <see cref="UserModel"/> class instance to be initialized.</param>
    /// <param name="modelBase">The <see cref="ModelBase"/> class instance of the User Details page.</param>
    /// <param name="id">The ID (database key) of the user to be displayed.</param>
    /// <returns>The <see cref="IActionResult"/> to be used to display the User Details page.</returns>
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
}
