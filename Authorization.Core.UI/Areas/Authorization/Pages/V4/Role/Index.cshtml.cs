using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role;

[RequiresClaims(SysClaims.Role.List)]
[PageImplementationType(typeof(IndexModel<,>))]
public abstract class IndexModel : ModelBase
{
    public IList<RoleInfo> RoleInfo { get; protected set; } = null!;

    public virtual Task OnGetAsync() => throw new NotImplementedException();
}

internal class IndexModel<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
    > : IndexModel
    where TRole : AuthUiRole
    where TUser : AuthUiUser
{
    private readonly IndexHandler<TUser, TRole> _indexHandler;

    /// <summary>
    /// Creates a new <see cref="IndexModel{TUser, TRole}"/> class instance using the specified parameters.
    /// </summary>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    public IndexModel(IRepository<TUser, TRole> repository)
    {
        _indexHandler = new IndexHandler<TUser, TRole>(repository);
    }

    public override async Task OnGetAsync()
    {
        RoleInfo = await _indexHandler.OnGetAsync().ConfigureAwait(false);
    }
}
