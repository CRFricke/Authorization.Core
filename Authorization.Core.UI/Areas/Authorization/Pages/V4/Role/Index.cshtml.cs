using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role
{
    [RequiresClaims(SysClaims.Role.List)]
    [PageImplementationType(typeof(IndexModel<,>))]
    public abstract class IndexModel : ModelBase
    {
        internal const string PageName = "Index";

        #region RoleModel Class

        public class RoleModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        #endregion

        public IList<RoleModel> Roles { get; set; }

        public virtual Task OnGetAsync() => throw new NotImplementedException();
    }

    internal class IndexModel<TUser, TRole> : IndexModel
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new IndexModel<TTUser,Role> class instance using the specified <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository">The repository instance to be used to initialize the IndexModel.</param>
        public IndexModel(IRepository<TUser,TRole> repository)
        {
            _repository = repository;
        }

        public override async Task OnGetAsync()
        {
            Roles = await _repository.Roles
                .AsNoTracking()
                .Select(ar => CreateRoleModel(ar))
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new RoleModel class instance using the specified TRole type instance.
        /// </summary>
        /// <param name="role">The role object to be used to initialize the RoleModel instance.</param>
        /// <returns>The new RoleModel instance.</returns>
        private static RoleModel CreateRoleModel(TRole role)
        {
            return new RoleModel
            {
                Id = role.Id,
                Description = role.Description,
                Name = role.Name
            };
        }
    }
}
