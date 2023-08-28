using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.Role;

[RequiresClaims(SysClaims.Role.Delete)]
[PageImplementationType(typeof(DeleteModel<,>))]
public abstract class DeleteModel : ModelBase
{
    [BindProperty]
    public RoleModel RoleModel { get; set; }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.New(ConstructorInfo, IEnumerable<Expression>, MemberInfo[]): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.New(ConstructorInfo, IEnumerable<Expression>, MemberInfo[]): The Property metadata or other accessor may be trimmed.")]
    public virtual Task<IActionResult> OnPostAsync(string id) => throw new NotImplementedException();
}

internal class DeleteModel<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : DeleteModel
    where TUser : AuthUiUser
    where TRole : AuthUiRole
{
    private readonly DeleteHandler<TUser, TRole> _deleteHandler;

    /// <summary>
    /// Creates a new <see cref="DeleteHandler{TUser, TRole}"/> class instance using the specified parameters.
    /// </summary>
    /// <param name="authManager">The <see cref="IAuthorizationManager"/> instance to be used for authorization.</param>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <param name="logger">The <see cref="ILogger{DeleteHandler}"/> instance to be used for logging.</param>
    public DeleteModel(
        IAuthorizationManager authManager,
        IRepository<TUser, TRole> repository,
        ILogger<DeleteHandler> logger)
    {
        _deleteHandler = new DeleteHandler<TUser, TRole>(authManager, repository, logger, typeof(IndexModel));
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.New(ConstructorInfo, IEnumerable<Expression>, MemberInfo[]): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnGetAsync(string id)
    {
        RoleModel = new RoleModel();
        return await _deleteHandler.OnGetAsync(RoleModel, this, id);
    }

    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.New(ConstructorInfo, IEnumerable<Expression>, MemberInfo[]): The Property metadata or other accessor may be trimmed.")]
    public override async Task<IActionResult> OnPostAsync(string id)
    {
        return await _deleteHandler.OnPostAsync(RoleModel, this, id);
    }
}
