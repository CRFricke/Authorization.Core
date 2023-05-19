using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Pages.Shared.User;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User
{
    [RequiresClaims(SysClaims.User.List)]
    [PageImplementationType(typeof(IndexModel<,>))]
    public abstract class IndexModel : ModelBase
    {
        public IList<UserInfo> UserInfo { get; set; }

        public virtual Task OnGetAsync() => throw new NotImplementedException();
    }

    internal class IndexModel<TUser, TRole> : IndexModel
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
}
