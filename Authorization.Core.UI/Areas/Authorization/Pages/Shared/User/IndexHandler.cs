using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace CRFricke.Authorization.Core.UI.Pages.Shared.User;

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
    /// Called to initialize the User Management page.
    /// </summary>
    /// <returns>The <see cref="IActionResult"/> to be used to display the User Management page.</returns>
    public async Task<IList<UserInfo>> OnGetAsync()
    {
        return await _repository.Users
            .AsNoTracking()
            .Select(au => new UserInfo().InitFromUser(au))
            .ToListAsync();
    }
}

#region UserInfo Class

public class UserInfo
{
    public string Id { get; set; } = null!;

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    [Display(Name = "Name")]
    public string? DisplayName { get; set; }

    [Display(Name = "Phone Number")]
    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Lockout Ends On")]
    [DisplayFormat(DataFormatString = "{0:o}")]
    [DataType(DataType.DateTime)]
    public DateTimeOffset? LockoutEnd { get; set; }

    [Display(Name = "Failed<br />Logins")]
    public int AccessFailedCount { get; set; }

    public override string ToString()
    {
        return Email;
    }

    internal UserInfo InitFromUser<TUser>(TUser user) where TUser : AuthUiUser
    {
        ArgumentNullException.ThrowIfNull(user);

        Id = user.Id;
        Email = user.Email!;
        DisplayName = user.DisplayName;
        PhoneNumber = user.PhoneNumber;
        LockoutEnd = user.LockoutEnd;
        AccessFailedCount = user.AccessFailedCount;

        return this;
    }
}

#endregion

internal class IndexHandler
{
    public const string PageName = "Index";
}
