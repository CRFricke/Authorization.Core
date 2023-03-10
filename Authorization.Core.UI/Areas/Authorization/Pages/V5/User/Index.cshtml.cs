using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User
{
    [RequiresClaims(SysClaims.User.List)]
    [PageImplementationType(typeof(IndexModel<,>))]
    public abstract class IndexModel : ModelBase
    {
        #region UserModel Class

        public class UserModel
        {
            public string Id { get; set; }

            [DataType(DataType.EmailAddress)]
            public string Email { get; set; }

            [Display(Name = "Name")]
            public string DisplayName { get; set; }

            [Display(Name = "Phone Number")]
            [DataType(DataType.PhoneNumber)]
            public string PhoneNumber { get; set; }

            [Display(Name = "Lockout Ends On")]
            [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}")]
            [DataType(DataType.DateTime)]
            public DateTimeOffset? LockoutEnd { get; set; }

            [Display(Name = "Failed<br />Logins")]
            public int AccessFailedCount { get; set; }

            public override string ToString()
            {
                return Email;
            }

            internal UserModel InitFromUser<TUser>(TUser user) where TUser : AuthUiUser
            {
                Id = user.Id;
                Email = user.Email;
                DisplayName = user.DisplayName;
                PhoneNumber = user.PhoneNumber;
                LockoutEnd = user.LockoutEnd;
                AccessFailedCount = user.AccessFailedCount;

                return this;
            }
        }

        #endregion

        internal const string PageName = "Index";

        public IList<UserModel> Users { get; set; }

        public virtual Task OnGetAsync() => throw new NotImplementedException();
    }

    internal class IndexModel<
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser, 
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
        > : IndexModel
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        private readonly IRepository<TUser, TRole> _repository;
        /// <summary>
        /// Creates a new IndexModel<TUser> class instance using the specified <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository">The repository instance to be used to initialize the IndexModel.</param>
        public IndexModel(IRepository<TUser, TRole> repository)
        {
            _repository = repository;
        }

        ///<inheritdoc/>
        public override async Task OnGetAsync()
        {
            Users = await _repository.Users
                .AsNoTracking()
                .Select(au => new UserModel().InitFromUser(au))
                .ToListAsync();
        }
    }
}
