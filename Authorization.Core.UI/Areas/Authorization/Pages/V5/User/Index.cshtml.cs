using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User;

[RequiresClaims(SysClaims.User.List)]
[PageImplementationType(typeof(IndexModel<,>))]
public abstract class IndexModel : ModelBase
{
    private static string _basePath = null;
    private readonly object _lockObject = new();

    public string BasePath
    {
        get
        {
            if (_basePath is null)
            {
                lock (_lockObject)
                {
                    if (_basePath is null)
                    {
                        var path = Request.Path.Value;
                        _basePath = (
                            path.EndsWith(IndexHandler.PageName) ? path[..(path.Length - IndexHandler.PageName.Length)] : path
                            ).TrimEnd('/');
                    }
                }
            }

            return _basePath;
        }
    }

    public IList<UserInfo> UserInfo { get; set; }

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

    ///<inheritdoc/>
    public override async Task OnGetAsync()
    {
        UserInfo = await _indexHandler.OnGetAsync();
    }
}
