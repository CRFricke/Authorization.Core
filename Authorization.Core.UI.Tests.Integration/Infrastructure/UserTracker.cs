using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Test.Support.Fakes;
using System;
using System.Linq;
using System.Security.Claims;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    internal class UserTracker : EntityTrackerBase
    {
        public UserTracker(AppUser user)
        {
            InitPropertyTracker(nameof(AccessFailedCount), user.AccessFailedCount);
            InitPropertyTracker(nameof(Email), user.Email);
            InitPropertyTracker(nameof(EmailConfirmed), user.EmailConfirmed);
            InitPropertyTracker(nameof(GivenName), user.GivenName);
            InitPropertyTracker(nameof(Id), user.Id);
            InitPropertyTracker(nameof(LockoutEnabled), user.LockoutEnabled);
            InitPropertyTracker(nameof(LockoutEnd), user.LockoutEnd);
            InitPropertyTracker(nameof(PhoneNumber), user.PhoneNumber);
            InitPropertyTracker(nameof(PhoneNumberConfirmed), user.PhoneNumberConfirmed);
            InitPropertyTracker(nameof(Surname), user.Surname);
            InitPropertyTracker(nameof(TwoFactorEnabled), user.TwoFactorEnabled);
            InitPropertyTracker(nameof(UserName), user.UserName);

            OriginalClaims = (
                from uc in user.Claims
                where uc.ClaimType == ClaimTypes.Role
                select uc.ClaimValue
                ).ToArray();

            CurrentClaims = OriginalClaims;
        }


        public string Id
        {
            get => GetValue<string>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Email
        {
            get => GetValue<string>(nameof(Email));
            set => SetValue(nameof(Email), value);
        }

        public string GivenName
        {
            get => GetValue<string>(nameof(GivenName));
            set => SetValue(nameof(GivenName), value);
        }

        public string Surname
        {
            get => GetValue<string>(nameof(Surname));
            set => SetValue(nameof(Surname), value);
        }

        public string PhoneNumber
        {
            get => GetValue<string>(nameof(PhoneNumber));
            set => SetValue(nameof(PhoneNumber), value);
        }

        public DateTimeOffset? LockoutEnd
        {
            get => GetValue<DateTimeOffset>(nameof(LockoutEnd));
            set => SetValue(nameof(LockoutEnd), value);
        }

        public int AccessFailedCount
        {
            get => GetValue<int>(nameof(AccessFailedCount));
            set => SetValue(nameof(AccessFailedCount), value);
        }

        public bool EmailConfirmed
        {
            get => GetValue<bool>(nameof(EmailConfirmed));
            set => SetValue(nameof(EmailConfirmed), value);
        }

        public bool PhoneNumberConfirmed
        {
            get => GetValue<bool>(nameof(PhoneNumberConfirmed));
            set => SetValue(nameof(PhoneNumberConfirmed), value);
        }

        public bool LockoutEnabled
        {
            get => GetValue<bool>(nameof(LockoutEnabled));
            set => SetValue(nameof(LockoutEnabled), value);
        }

        public bool TwoFactorEnabled
        {
            get => GetValue<bool>(nameof(TwoFactorEnabled));
            set => SetValue(nameof(TwoFactorEnabled), value);
        }

        public string UserName
        {
            get => GetValue<string>(nameof(UserName));
            set => SetValue(nameof(UserName), value);
        }


        public UserTracker SetValue<TProperty>(string key, TProperty value) where TProperty : IEquatable<TProperty>
        {
            base.SetValue<TProperty>(key, value);
            return this;
        }

        public UserTracker SetValue<TProperty>(string key, TProperty? value) where TProperty : struct, IEquatable<TProperty>
        {
            base.SetValue<TProperty>(key, value);
            return this;
        }

        public UserTracker SetClaims(params string[] claims)
        {
            CurrentClaims = claims;
            return this;
        }
    }
}