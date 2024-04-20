using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User;

[RequiresClaims(SysClaims.User.Create)]
[PageImplementationType(typeof(CreateModel<,>))]
public abstract class CreateModel : ModelBase
{
    [BindProperty]
    public UserModel UserModel { get; set; }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnGetAsync() => throw new NotImplementedException();

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
}

internal class CreateModel<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : CreateModel
    where TUser : AuthUiUser, new()
    where TRole : AuthUiRole
{
    private readonly CreateHandler<TUser, TRole> _createHandler;

    /// <summary>
    /// Creates a new <see cref="CreateModel{TUser, TRole}"/> class instance using the specified parameters.
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="userManager">The <see cref="UserManager{TUser}"/> instance to be used for user validation.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{CreateHandler}"/> instance to be used for logging.</param>
    public CreateModel(
        IAuthorizationManager authManager, 
        UserManager<TUser> userManager,
        IRepository<TUser, TRole> repository, 
        ILogger<CreateHandler> logger)
    {
        _createHandler = new CreateHandler<TUser, TRole>(authManager, userManager, repository, logger, typeof(IndexModel));
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnGetAsync()
    {
        UserModel = new();
        return await _createHandler.OnGetAsync(UserModel, this);
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnPostAsync(string hfRoleList)
    {
        return await _createHandler.OnPostAsync(UserModel, this, hfRoleList);
    }
}
