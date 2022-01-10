using Authorization.Core.UI.Tests.Data;
using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI;
using CRFricke.Authorization.Core.UI.Models;
using System.Collections.Generic;

namespace Authorization.Core.UI.Tests
{
    public class TestsBase
    {

        /// <summary>
        /// Creates a new <see cref="RoleModel"/> object using the specified <see cref="ApplicationRole"/>.
        /// </summary>
        /// <param name="role">The <see cref="ApplicationRole"/> object to be used to initialize the <see cref="RoleModel"/>.</param>
        /// <returns>The new <see cref="RoleModel"/> object.</returns>
        internal RoleModel CreateModelFromRole(ApplicationRole role)
        {
            return new RoleModel { Id = role.Id, Description = role.Description, Name = role.Name };
        }

        /// <summary>
        /// Creates a new <see cref="UserModel"/> object using the specified <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="role">The <see cref="ApplicationUser"/> object to be used to initialize the <see cref="UserModel"/>.</param>
        /// <returns>The new <see cref="UserModel"/> object.</returns>
        internal UserModel CreateModelFromUser(ApplicationUser user)
        {
            return new UserModel {
                Id = user.Id, AccessFailedCount = user.AccessFailedCount, Email = user.Email, EmailConfirmed = user.EmailConfirmed,
                GivenName = user.GivenName, LockoutEnabled = user.LockoutEnabled, LockoutEnd = user.LockoutEnd,
                PhoneNumber = user.PhoneNumber, PhoneNumberConfirmed = user.PhoneNumberConfirmed, Surname = user.Surname
            };
        }

        /// <summary>
        /// Returns the list of defined claims.
        /// </summary>
        /// <returns>The list of defined claims.</returns>
        internal List<string> GetDefinedClaims()
        {
            return new List<string>
            {
                SysClaims.Role.Create, SysClaims.Role.Delete, SysClaims.Role.List, SysClaims.Role.Read, SysClaims.Role.Update,
                SysClaims.User.Create, SysClaims.User.Delete, SysClaims.User.List, SysClaims.User.Read, SysClaims.User.Update
            };
        }

        /// <summary>
        /// Returns the list of defined Guids.
        /// </summary>
        /// <returns>The list of defined Guids.</returns>
        internal List<string> GetDefinedGuids()
        {
            return new List<string>
            {
                SysGuids.Role.Administrator, SysUiGuids.Role.RoleManager, SysUiGuids.Role.UserManager,
                SysGuids.User.Administrator
            };
        }
    }
}