using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.Shared.Role;

internal class IndexHandler<TUser, TRole>
    where TRole : AuthUiRole
    where TUser : AuthUiUser
{
    private readonly IRepository<TUser, TRole> _repository;

    /// <summary>
    /// Creates a new IndexHandler class instance using the specified <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> instance to be used to initialize the IndexHandler.</param>
    public IndexHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<TUser, TRole>>();
    }

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
    /// Creates a new RoleInfo class instance using the specified <typeparamref name="TRole"/> object.
    /// </summary>
    /// <typeparam name="TRole">A class that derives from (or is the) <see cref="AuthUiRole"/> class.</typeparam>
    /// <param name="role">A <typeparamref name="TRole"/> class instance to be used to initialize the RoleInfo object.</param>
    /// <returns>The new RoleInfo class object.</returns>
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
