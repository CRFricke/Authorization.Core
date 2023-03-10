using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V5.User
{
    [RequiresClaims(SysClaims.User.Delete)]
    [PageImplementationType(typeof(DeleteModel<,>))]
    public abstract class DeleteModel : ModelBase
    {
        [BindProperty]
        public bool IsSystemUser { get; protected set; }

        [BindProperty]
        public UserModel UserModel { get; set; }

        [RequiresUnreferencedCode("The Property metadata or other accessor may be trimmed.")]
        public virtual Task<IActionResult> OnGetAsync(string id) => throw new NotImplementedException();

        [RequiresUnreferencedCode("The Property metadata or other accessor may be trimmed.")]
        public virtual Task<IActionResult> OnPostAsync(string id) => throw new NotImplementedException();
    }

    internal class DeleteModel<
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TUser, 
        [DynamicallyAccessedMembers(IRepository.DynamicallyAccessedMemberTypes)] TRole
        > : DeleteModel
        where TUser : AuthUiUser
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<DeleteModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new EditModel<TUser> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the EditModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the EditModel.</param>
        public DeleteModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<DeleteModel> logger)
        {
            _authManager = authManager;
            _logger = logger;
            _repository = repository;
        }

        [RequiresUnreferencedCode("The Property metadata or other accessor may be trimmed.")]
        public override async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repository.Users
                .Include(au => au.Claims)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            IsSystemUser = _authManager.DefinedGuids.Contains(user.Id);

            UserModel = (await new UserModel()
                .InitRoleInfoAsync(_repository))
                .InitFromUser(user);

            return Page();
        }

        [RequiresUnreferencedCode("The Property metadata or other accessor may be trimmed.")]
        public override async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repository.Users
                .Include(au => au.Claims)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                SendNotification(typeof(IndexModel), Severity.High,
                    $"Error: User '{UserModel.Email}' was not found in the database. Another user may have deleted it."
                    );

                return RedirectToPage(IndexModel.PageName);
            }

            // Don't care about ModelState on Delete.
            ModelState.Clear();

            var result = await _authManager.AuthorizeAsync(User, user, new AppClaimRequirement(SysClaims.User.Delete));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can not delete User:");
                ModelState.AddModelError(string.Empty, "System accounts may not be deleted.");

                _logger.LogWarning(
                    "'{PrincipalEmail}' attempted to delete system {UserType} '{UserEmail}' (ID '{UserId}').",
                    User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                    );

                (await UserModel.InitRoleInfoAsync(_repository)).InitFromUser(user);
                return Page();
            }

            try
            {
                _repository.Users.Remove(user);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not delete User:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(
                    ex, "'{PrincipalEmail}' could not delete {UserType} '{UserEmail}' (ID '{UserId}').",
                    User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                    );
            }

            if (!ModelState.IsValid)
            {
                (await UserModel.InitRoleInfoAsync(_repository)).InitFromUser(user);
                return Page();
            }

            // Remove User from UserClaims cache
            _authManager.RefreshUser(user.Id);

            SendNotification(
                typeof(IndexModel), Severity.Normal,
                $"User '{user.Email}' successfully deleted."
                );

            _logger.LogInformation(
                "'{PrincipalEmail}' deleted {UserType} '{UserEmail}' (ID '{UserId}').",
                User.Identity.Name, typeof(TUser).Name, user.Email, user.Id
                );

            return RedirectToPage(IndexModel.PageName);
        }
    }
}
