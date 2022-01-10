using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Test.Support.Fakes;
using System;
using System.Linq;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    internal class RoleTracker : EntityTrackerBase
    {
        public RoleTracker(AuthUiRole role)
        {
            InitPropertyTracker(nameof(Id), role.Id);
            InitPropertyTracker(nameof(Name), role.Name);
            InitPropertyTracker(nameof(Description), role.Description);

            OriginalClaims = (
                from rc in role.Claims
                where rc.ClaimType == SysClaims.ClaimType
                select rc.ClaimValue
                ).ToArray();

            CurrentClaims = OriginalClaims;
        }

        public string Id
        {
            get => GetValue<string>(nameof(Id));
            set => SetValue(nameof(Id), value);
        }

        public string Name
        {
            get => GetValue<string>(nameof(Name));
            set => SetValue(nameof(Name), value);
        }

        public string Description
        {
            get => GetValue<string>(nameof(Description));
            set => SetValue(nameof(Description), value);
        }

        public RoleTracker SetValue<TProperty>(string key, TProperty value) where TProperty: IEquatable<TProperty>
        {
            base.SetValue<TProperty>(key, value);
            return this;
        }

        public RoleTracker SetClaims(params string[] claims)
        {
            CurrentClaims = claims;
            return this;
        }
    }
}
