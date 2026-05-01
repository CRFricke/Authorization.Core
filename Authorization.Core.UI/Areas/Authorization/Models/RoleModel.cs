using CRFricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Claims;

#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable CA1034 // Nested types should not be visible

namespace CRFricke.Authorization.Core.UI.Models;

public class RoleModel
{
    #region RoleClaim Class

    /// <summary>
    /// Represents a claim that can be assigned to a role.
    /// </summary>
    public class RoleClaim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoleClaim"/> class.
        /// </summary>
        public RoleClaim()
        { }

        /// <summary>
        /// Gets or sets the claim value.
        /// </summary>
        public string Claim { get; set; } = null!;

        /// <summary>
        /// Gets or sets the unique identifier for this role claim.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether this claim is assigned to the role.
        /// </summary>
        [Display(Name = "Select")]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// Returns a string representation of this role claim.
        /// </summary>
        /// <returns>A string containing the claim value and assignment status.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}",
                Claim, IsAssigned ? " (assigned)" : string.Empty
                );
        }
    }

    #endregion

    #region RoleUser class

    /// <summary>
    /// Represents a user that has been assigned to a role.
    /// </summary>
    public class RoleUser
    {
        /// <summary>
        /// Gets or sets the display name of the user.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; } = null!;
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleModel"/> class.
    /// </summary>
    public RoleModel()
    { }


    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    [Required]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the collection of claims that can be assigned to this role.
    /// </summary>
    public IReadOnlyCollection<RoleClaim>? RoleClaims { get; set; }

    /// <summary>
    /// Gets or sets the collection of users assigned to this role.
    /// </summary>
    public IReadOnlyCollection<RoleUser>? RoleUsers { get; set; }

    /// <summary>
    /// Gets a value indicating whether the role's claims were updated during the last <see cref="UpdateRole"/> operation.
    /// </summary>
    public bool ClaimsUpdated { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a system-defined role.
    /// </summary>
    public bool IsSystemRole { get; set; }


    /// <summary>
    /// Initializes the <see cref="RoleClaims"/> collection with all defined claims from the authorization manager.
    /// </summary>
    /// <param name="authManager">The authorization manager containing the defined claims.</param>
    /// <returns>This <see cref="RoleModel"/> instance for method chaining.</returns>
    internal RoleModel InitRoleClaims(IAuthorizationManager authManager)
    {
        RoleClaims = (
            from claim in authManager.DefinedClaims
            select new RoleClaim { Claim = claim, IsAssigned = false }
            ).ToList();

        return this;
    }

    /// <summary>
    /// Initializes this <see cref="RoleModel"/> from an existing <see cref="AuthUiRole"/>.
    /// </summary>
    /// <param name="role">The role to initialize from.</param>
    /// <returns>This <see cref="RoleModel"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is null.</exception>
    public virtual RoleModel InitFromRole(AuthUiRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

        Id = role.Id;
        Name = role.Name!;
        Description = role.Description;

        return SetAssignedClaims(role.Claims);
    }

    /// <summary>
    /// Initializes the <see cref="RoleUsers"/> collection by querying the repository for users assigned to this role.
    /// </summary>
    /// <typeparam name="TUser">The type of user entity.</typeparam>
    /// <typeparam name="TRole">The type of role entity.</typeparam>
    /// <param name="repository">The repository to query for user assignments.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains this <see cref="RoleModel"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="repository"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="InitFromRole"/> has not been called before this method.</exception>
    [RequiresUnreferencedCode("System.Linq.Expressions.Expression.New(ConstructorInfo, IEnumerable<Expression>, MemberInfo[]): The Property metadata or other accessor may be trimmed.")]
    public virtual async Task<RoleModel> InitRoleUsersAsync<
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
        >(IRepository<TUser, TRole> repository)
        where TRole : AuthUiRole
        where TUser : AuthUiUser
    {
        ArgumentNullException.ThrowIfNull(repository);

        _ = Id ?? throw new InvalidOperationException(
            $"{nameof(InitFromRole)} has not been called."
            );

        RoleUsers = await (
            from uc in repository.UserClaims
            join au in repository.Users on uc.UserId equals au.Id
            where uc.ClaimType == ClaimTypes.Role && uc.ClaimValue == Id
            select new RoleUser { Name = au.DisplayName, Email = au.Email! }
            ).ToListAsync().ConfigureAwait(false);

        return this;
    }

    /// <summary>
    /// Gets the collection of claim values that are currently assigned to this role.
    /// </summary>
    /// <returns>A collection of claim values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="InitRoleClaims"/> has not been called.</exception>
    internal ICollection<string> GetAssignedClaims()
    {
        VerifyClaimsLoaded();

        return (
            from claim in RoleClaims
            where claim.IsAssigned
            select claim.Claim
            ).ToArray();
    }

    /// <summary>
    /// Sets the assigned claims from a collection of <see cref="IdentityRoleClaim{TKey}"/>.
    /// </summary>
    /// <param name="roleClaims">The collection of role claims to process.</param>
    /// <returns>This <see cref="RoleModel"/> instance for method chaining.</returns>
    private RoleModel SetAssignedClaims(ICollection<IdentityRoleClaim<string>> roleClaims)

    {
        SetAssignedClaims(
            from claim in roleClaims
            select claim.ClaimValue
            );

        return this;
    }

    /// <summary>
    /// Sets the assigned claims by marking which claims in the <see cref="RoleClaims"/> collection are assigned.
    /// </summary>
    /// <param name="claims">The collection of claim values to mark as assigned.</param>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="InitRoleClaims"/> has not been called.</exception>
    internal void SetAssignedClaims(IEnumerable<string> claims)
    {
        VerifyClaimsLoaded();

        var claimsSet = claims.ToHashSet();

        foreach (var roleClaim in RoleClaims!)
        {
            roleClaim.IsAssigned = claimsSet.Contains(roleClaim.Claim);
        }
    }

    /// <summary>
    /// Updates the specified <see cref="AuthUiRole"/> with the values from this model.
    /// </summary>
    /// <param name="role">The role to update.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="role"/> is null.</exception>
    /// <remarks>
    /// This method updates the role's name, description, and claims. The <see cref="ClaimsUpdated"/> property
    /// is set to indicate whether the claims were modified.
    /// </remarks>
    public virtual void UpdateRole(AuthUiRole role)
    {
        ArgumentNullException.ThrowIfNull(role);

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

    /// <summary>
    /// Verifies that the <see cref="RoleClaims"/> collection has been initialized.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="InitRoleClaims"/> has not been called.</exception>
    private void VerifyClaimsLoaded()
    {
        _ = RoleClaims ?? throw new InvalidOperationException(
            $"{nameof(InitRoleClaims)} has not been called."
            );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="RoleModel"/>.
    /// </summary>
    /// <returns>The role name.</returns>
    public override string ToString()
    {
        return Name;
    }
}
