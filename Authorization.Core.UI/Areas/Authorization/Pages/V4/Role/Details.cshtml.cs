using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role;

[RequiresClaims(SysClaims.Role.Read)]
[PageImplementationType(typeof(DetailsModel<,>))]
public abstract class DetailsModel : ModelBase
{
    public RoleModel RoleModel { get; set; }

    public virtual Task<IActionResult> OnGetAsync(string id)
        => throw new NotImplementedException();
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

    public override async Task<IActionResult> OnGetAsync(string id)
    {
        RoleModel = new RoleModel();
        return await _detailsHandler.OnGetAsync(RoleModel, this, id);
    }
}
