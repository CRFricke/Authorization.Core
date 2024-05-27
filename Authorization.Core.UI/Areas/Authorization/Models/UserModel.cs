using CRFricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Models
{
    public class UserModel
    {
        public UserModel() { }

        #region RoleInfo Class

        public class RoleInfo
        {
            public RoleInfo()
            { }

            public string Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            [Display(Name = "Select")]
            public bool IsAssigned { get; set; }

            public override string ToString()
            {
                return string.Format("{0}{1}",
                    Name, IsAssigned ? " (assigned)" : string.Empty
                    );
            }
        }

        #endregion

        public string Id { get; set; }

        [Display(Name = "Failed Logins")]
        public int AccessFailedCount { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; } = true;

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; } = true;

        [Display(Name = "Lockout Ends On (UTC)")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:M/d/yyyy h:m:ss tt K}")]
        public DateTimeOffset? LockoutEndUtc { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "Referenced property is a string.")]
        public string ConfirmPassword { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Phone Number Confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Display(Name = "First Name")]
        public string GivenName { get; set; }

        [Display(Name = "Last Name")]
        public string Surname { get; set; }

        public ICollection<RoleInfo> Roles { get; private set; }

        public bool ClaimsUpdated { get; private set; }

        public bool IsSystemUser { get; set; }

        public override string ToString()
        {
            return Email;
        }

        /// <summary>
        /// Initializes this <see cref="UserModel"/> object using the specified <typeparamref name="TUser"/> class instance.
        /// </summary>
        /// <typeparam name="TUser">A <see cref="Type"/> that extends <see cref="AppUserBase"/>.</typeparam>
        /// <param name="user">
        /// The <typeparamref name="TUser"/> class instance to be used to initialize this <see cref="UserModel"/> object.
        /// </param>
        /// <returns>The initialized <see cref="UserModel"/> object.</returns>
        public virtual UserModel InitFromUser<TUser>(TUser user) where TUser : AuthUiUser
        {
            Id = user.Id;
            AccessFailedCount = user.AccessFailedCount;
            Email = user.Email;
            EmailConfirmed = user.EmailConfirmed;
            LockoutEnabled = user.LockoutEnabled;
            LockoutEndUtc = user.LockoutEnd;
            Password = "Only used in create mode";
            ConfirmPassword = Password;
            PhoneNumber = user.PhoneNumber;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            GivenName = user.GivenName;
            Surname = user.Surname;

            SetAssignedClaims(user.Claims);

            return this;
        }

        [RequiresUnreferencedCode("System.Linq.Expressions.Expression.Bind(MethodInfo, Expression): The Property metadata or other accessor may be trimmed.")]
        public virtual async Task<UserModel> InitRoleInfoAsync<
            [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser, 
            [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
            >(IRepository<TUser, TRole> repository)
            where TRole : AuthUiRole
            where TUser : AuthUiUser
        {
            Roles = await (
                from ar in repository.Roles
                select new RoleInfo { Description = ar.Description, Id = ar.Id, IsAssigned = false, Name = ar.Name }
                ).ToArrayAsync();

            return this;
        }

        internal ICollection<string> GetAssignedClaims()
        {
            VerifyRolesLoaded();

            return (
                from role in Roles
                where role.IsAssigned
                select role.Id
                ).ToArray();
        }

        private void SetAssignedClaims(ICollection<IdentityUserClaim<string>> claims)
        {
            SetAssignedClaims(
                from claim in claims
                select claim.ClaimValue
                );
        }

        internal void SetAssignedClaims(IEnumerable<string> roles)
        {
            VerifyRolesLoaded();

            foreach (var role in Roles)
            {
                role.IsAssigned = roles.Contains(role.Id);
            }
        }

        public virtual void UpdateUser<TUser>(TUser user) where TUser : AuthUiUser
        {
            var normalizer = new UpperInvariantLookupNormalizer();

            if (AccessFailedCount != user.AccessFailedCount)
            {
                user.AccessFailedCount = AccessFailedCount;
            }

            if (Email != user.Email)
            {
                user.Email = Email;
                user.NormalizedEmail = normalizer.NormalizeEmail(Email);

                user.UserName = Email;
                user.NormalizedUserName = user.NormalizedEmail;
            }

            if (EmailConfirmed != user.EmailConfirmed)
            {
                user.EmailConfirmed = EmailConfirmed;
            }

            if (GivenName != user.GivenName)
            {
                user.GivenName = GivenName;
            }

            if (LockoutEnabled != user.LockoutEnabled)
            {
                user.LockoutEnabled = LockoutEnabled;
            }

            if (LockoutEndUtc != user.LockoutEnd)
            {
                user.LockoutEnd = LockoutEndUtc;
            }

            if (PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = PhoneNumber;
            }

            if (PhoneNumberConfirmed != user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = PhoneNumberConfirmed;
            }

            if (Surname != user.Surname)
            {
                user.Surname = Surname;
            }

            ClaimsUpdated = user.UpdateClaims(GetAssignedClaims());
        }

        private void VerifyRolesLoaded()
        {
            _ = Roles ?? throw new InvalidOperationException(
                $"{nameof(InitRoleInfoAsync)} has not been called yet."
                );
        }
    }
}
