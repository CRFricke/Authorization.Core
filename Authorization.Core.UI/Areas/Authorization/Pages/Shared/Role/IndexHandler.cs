using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class IndexHandler<
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser,
    [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole>
    where TRole : AuthUiRole
    where TUser : AuthUiUser
{
    private readonly IRepository<TUser, TRole> _repository;

    /// <summary>
    /// Creates a new <see cref="IndexHandler{TUser, TRole}"/> class instance using the specified parameters.
    /// </summary>
    /// <param name="repository">The <see cref="IRepository{TUser, TRole}"/> instance to be used for database access.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="repository"/> is <see langword="null"/>.
    /// </exception>
    public IndexHandler(IRepository<TUser, TRole> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Called to initialize the Role Management page.
    /// </summary>
    /// <returns>The <see cref="IActionResult"/> to be used to display the Role Management page.</returns>
    public async Task<IList<RoleInfo>> OnGetAsync()
    {
        return await _repository.Roles
            .AsNoTracking()
            .Select(ar => RoleInfo.Create(ar))
            .ToListAsync();
    }

}

/// <summary>
/// Describes an <see cref="AuthUiRole"/> object.
/// </summary>
public class RoleInfo
{
    /// <summary>
    /// Creates a new <see cref="RoleInfo"/> class instance using the specified <typeparamref name="TRole"/> object.
    /// </summary>
    /// <typeparam name="TRole">A class that derives from (or is) the <see cref="AuthUiRole"/> class.</typeparam>
    /// <param name="role">A <typeparamref name="TRole"/> class instance to be used to initialize the RoleInfo object.</param>
    /// <returns>The new <see cref="RoleInfo"/> class object.</returns>
    internal static RoleInfo Create<TRole>(TRole role) where TRole : AuthUiRole
    {
        return new RoleInfo
        {
            Id = role.Id,
            Description = role.Description,
            Name = role.Name
        };
    }

    public string Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public override string ToString()
    {
        return Name;
    }
}

internal class IndexHandler
{
    public const string PageName = "Index";
}
