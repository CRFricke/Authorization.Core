using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User;

[RequiresClaims(SysClaims.User.Update)]
[PageImplementationType(typeof(EditModel<,>))]
public abstract class EditModel : ModelBase
{
    [BindProperty]
    public UserModel UserModel { get; set; }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
}

internal class EditModel<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : EditModel
    where TUser : AuthUiUser
    where TRole : AuthUiRole
{
    private readonly EditHandler<TUser, TRole> _editHandler;

    /// <summary>
    /// Creates a new EditModel<TUser> class instance using the specified authorization manager and repository.
    /// </summary>
    /// <param name="authManager">The AuthorizationManager instance to be used to initialize the EditModel.</param>
    /// <param name="repository">The repository instance to be used to initialize the EditModel.</param>
    /// <param name="logger">The <see cref="ILogger{EditHandler}"/> instance to be used for logging.</param>
    public EditModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<EditHandler> logger)
    {
        _editHandler = new EditHandler<TUser, TRole>(authManager, repository, logger, typeof(IndexModel));
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnGetAsync(string id)
    {
        UserModel = new();
        return await _editHandler.OnGetAsync(UserModel, this, id);
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnPostAsync(string hfRoleList)
    {
        return await _editHandler.OnPostAsync(UserModel, this, hfRoleList);
    }
}
