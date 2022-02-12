using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.User
{
    [RequiresClaims(SysClaims.User.Create)]
    [PageImplementationType(typeof(CreateModel<,>))]
    public abstract class CreateModel : ModelBase
    {
        [BindProperty]
        public UserModel UserModel { get; set; }

        public virtual Task<IActionResult> OnGetAsync() => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfRoleList) => throw new NotImplementedException();
    }

    internal class CreateModel<TUser, TRole> : CreateModel
        where TUser : AuthUiUser, new()
        where TRole : AuthUiRole
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<CreateModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new CreateModel<TRole> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the CreateModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the CreateModel.</param>
        public CreateModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<CreateModel> logger)
        {
            _authManager = authManager;
            _logger = logger;
            _repository = repository;
        }

        public override async Task<IActionResult> OnGetAsync()
        {
            UserModel = await new UserModel()
                .InitRoleInfoAsync(_repository);

            return Page();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        public override async Task<IActionResult> OnPostAsync(string hfRoleList)
        {
            (await UserModel.InitRoleInfoAsync(_repository))
                .SetAssignedClaims(hfRoleList?.Split(',') ?? Array.Empty<string>());

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new TUser();
            UserModel.UpdateUser(user);

            var result = await _authManager.AuthorizeAsync(User, user, new AppClaimRequirement(SysClaims.User.Create));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can not create User:");
                ModelState.AddModelError(string.Empty, "You can not create a User with more privileges than you have.");

                _logger.LogInformation($"User '{User.Identity.Name}' (ID '{User.UserId()}') attempted to give {typeof(TUser).Name} elevated privileges.");

                return Page();
            }

            user.PasswordHash = new PasswordHasher<TUser>()
                .HashPassword(user, UserModel.Password);

            try
            {
                _repository.Users.Add(user);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not create User:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(ex, $"Could not create {typeof(TUser).Name} '{user.Email}' (ID '{user.Id}').");

                return Page();
            }

            SendNotification(
                typeof(IndexModel), Severity.Normal,
                $"User '{user.Email}' successfully created."
                );

            _logger.LogInformation($"{typeof(TUser).Name} '{user.Email}' (ID '{user.Id}') was created.");

            return RedirectToPage(IndexModel.PageName);
        }
    }
}
