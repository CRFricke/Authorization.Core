using CRFricke.Authorization.Core.UI.Data;
using System;

namespace Authorization.Core.UI.Tests.Integration.Models
{
    public class UserModel
    {
        public string Id { get; internal set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string GivenName { get; set; }

        public string Surname { get; set; }

        public string PhoneNumber { get; set; }

        public DateTimeOffset? LockoutEnd { get; private set; }

        public int AccessFailedCount { get; private set; }

        public bool EmailConfirmed { get; private set; }

        public bool PhoneNumberConfirmed { get; private set; }

        public bool LockoutEnabled { get; set; }


        internal static UserModel CreateFrom(AuthUiUser user)
        {
            return new UserModel
            {
                AccessFailedCount = user.AccessFailedCount,
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                GivenName = user.GivenName,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Surname = user.Surname
            };
        }

        internal AuthUiUser ToUser()
        {
            return new AuthUiUser
            {
                AccessFailedCount = AccessFailedCount,
                Id = Id ?? Guid.NewGuid().ToString(),
                Email = Email,
                EmailConfirmed = EmailConfirmed,
                GivenName = GivenName,
                LockoutEnabled = LockoutEnabled,
                LockoutEnd = LockoutEnd,
                PhoneNumber = PhoneNumber,
                PhoneNumberConfirmed = PhoneNumberConfirmed,
                Surname = Surname
            };
        }
    }
}
