using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User;

[RequiresClaims(SysClaims.User.Read)]
[PageImplementationType(typeof(DetailsModel<,>))]
public abstract class DetailsModel : ModelBase
{
    public UserModel UserModel { get; set; }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();
}

internal class DetailsModel<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : DetailsModel
    where TUser : AuthUiUser
    where TRole : AuthUiRole
{
    private readonly DetailsHandler<TUser, TRole> _detailsHandler;

    /// <summary>
    /// Creates a new <see cref="DetailsModel{TUser, TRole}"/> class instance using the specified authorization manager and repository.
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    public DetailsModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository)
    {
        _detailsHandler = new DetailsHandler<TUser, TRole>(authManager, repository);
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnGetAsync(string id)
    {
        UserModel = new();
        return await _detailsHandler.OnGetAsync(UserModel, this, id);
    }
}
