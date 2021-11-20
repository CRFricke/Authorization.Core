using Fricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Fricke.Authorization.Core.UI.Models
{
    public class RoleModel
    {
        #region RoleClaim Class

        public class RoleClaim
        {
            public RoleClaim()
            { }

            public string Claim { get; set; }

            public int Id { get; internal set; }

            [Display(Name = "Select")]
            public bool IsAssigned { get; set; }

            public override string ToString()
            {
                return string.Format("{0}{1}",
                    Claim, IsAssigned ? " (assigned)" : string.Empty
                    );
            }
        }

        #endregion

        #region RoleUser class

        public class RoleUser
        {
            public string Name { get; set; }

            public string Email { get; set; }
        }

        #endregion

        public RoleModel()
        { }


        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public List<RoleClaim> RoleClaims { get; set; }

        public List<RoleUser> RoleUsers { get; set; }

        public bool ClaimsUpdated { get; private set; }


        internal RoleModel InitRoleClaims(IAuthorizationManager authManager)
        {
            RoleClaims = (
                from claim in authManager.DefinedClaims
                select new RoleClaim { Claim = claim, IsAssigned = false }
                ).ToList();

            return this;
        }

        public virtual RoleModel InitFromRole(AppRole role)
        {
            Id = role.Id;
            Name = role.Name;
            Description = role.Description;

            return SetAssignedClaims(role.Claims);
        }

        public virtual async Task<RoleModel> InitRoleUsersAsync<TUser, TRole>(IRepository<TUser, TRole> repository)
            where TRole : AppRole
            where TUser : AppUser
        {
            _ = Name ?? throw new InvalidOperationException(
                $"{nameof(InitFromRole)} has not been called."
                );

            RoleUsers = await (
                from uc in repository.UserClaims
                join au in repository.Users on uc.UserId equals au.Id
                where uc.ClaimType == ClaimTypes.Role && uc.ClaimValue == Name
                select new RoleUser { Name = au.DisplayName, Email = au.Email }
                ).ToListAsync();

            return this;
        }

        internal ICollection<string> GetAssignedClaims()
        {
            VerifyClaimsLoaded();

            return (
                from claim in RoleClaims
                where claim.IsAssigned
                select claim.Claim
                ).ToArray();
        }

        private RoleModel SetAssignedClaims(ICollection<IdentityRoleClaim<string>> roleClaims)

        {
            SetAssignedClaims(
                from claim in roleClaims
                select claim.ClaimValue
                );

            return this;
        }

        internal void SetAssignedClaims(IEnumerable<string> claims)
        {
            VerifyClaimsLoaded();

            foreach (var roleClaim in RoleClaims)
            {
                roleClaim.IsAssigned = claims.Contains(roleClaim.Claim);
            }
        }

        public virtual void UpdateRole(AppRole role)
        {
            var normalizer = new UpperInvariantLookupNormalizer();

            if (Description != role.Description)
            {
                role.Description = Description;
            }

            if (Name != role.Name)
            {
                role.Name = Name;
                role.NormalizedName = normalizer.NormalizeName(Name);
            }

            ClaimsUpdated = role.UpdateClaims(GetAssignedClaims());
        }

        private void VerifyClaimsLoaded()
        {
            _ = RoleClaims ?? throw new InvalidOperationException(
                $"{nameof(InitRoleClaims)} has not been called."
                );
        }

        /// <summary>
        /// Returns a representation of this <see cref="RoleModel"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
