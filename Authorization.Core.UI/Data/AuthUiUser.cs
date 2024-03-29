﻿using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Identity;

namespace CRFricke.Authorization.Core.UI.Data
{
    public class AuthUiUser : AuthUser
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthUiUser"/> class.
        /// </summary>
        public AuthUiUser()
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="AuthUiUser"/> class with the specified email address.
        /// </summary>
        /// <param name="email">The Email address of the <see cref="AuthUiUser"/>.</param>
        public AuthUiUser(string email) : base(email)
        { }

        /// <summary>
        /// The user's given (first) name.
        /// </summary>
        [ProtectedPersonalData]
        public virtual string GivenName { get; set; }

        /// <summary>
        /// The user's surname (last name).
        /// </summary>
        [ProtectedPersonalData]
        public virtual string Surname { get; set; }

        /// <summary>
        /// Returns the display name of the user.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return $"{Surname}, {GivenName}".Trim(',', ' ');
            }
        }
    }
}
