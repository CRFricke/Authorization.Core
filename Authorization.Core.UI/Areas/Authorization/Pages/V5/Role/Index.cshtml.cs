using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Pages.Shared.Role;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.Role
{
    [RequiresClaims(SysClaims.Role.List)]
    [PageImplementationType(typeof(IndexModel<,>))]
    public abstract class IndexModel : ModelBase
    {
        public IList<RoleInfo> RoleInfo { get; set; }

        public virtual Task OnGetAsync() => throw new NotImplementedException();
    }

    internal class IndexModel<TUser, TRole> : IndexModel
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        private readonly IndexHandler<TUser, TRole> _indexHandler;

        /// <summary>
        /// Creates a new IndexModel&lt;TUser, TRole&gt; class instance using the specified <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the DetailsModel.</param>
        public IndexModel(IServiceProvider serviceProvider)
        {
            _indexHandler = new IndexHandler<TUser, TRole>(serviceProvider);
        }

        public override async Task OnGetAsync()
        {
            RoleInfo = await _indexHandler.OnGetAsync();
        }
    }
}
