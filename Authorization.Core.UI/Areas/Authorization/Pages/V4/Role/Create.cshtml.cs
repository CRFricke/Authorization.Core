using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.UI.Data;
using CRFricke.Authorization.Core.UI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core.UI.Pages.V4.Role
{
    [RequiresClaims(SysClaims.Role.Create)]
    [PageImplementationType(typeof(CreateModel<,>))]
    public abstract class CreateModel : ModelBase
    {
        [BindProperty]
        public RoleModel RoleModel { get; set; }

        public virtual IActionResult OnGet() => throw new NotImplementedException();

        public virtual Task<IActionResult> OnPostAsync(string hfClaimList) => throw new NotImplementedException();
    }

    internal class CreateModel<TUser, TRole> : CreateModel
        where TRole : AuthUiRole, new()
        where TUser : AuthUiUser
    {
        private readonly IAuthorizationManager _authManager;
        private readonly ILogger<CreateModel> _logger;
        private readonly IRepository<TUser, TRole> _repository;

        /// <summary>
        /// Creates a new CreateModel<TUser, TRole> class instance using the specified authorization manager and repository.
        /// </summary>
        /// <param name="authManager">The AuthorizationManager instance to be used to initialize the CreateModel.</param>
        /// <param name="repository">The repository instance to be used to initialize the CreateModel.</param>
        public CreateModel(IAuthorizationManager authManager, IRepository<TUser, TRole> repository, ILogger<CreateModel> logger)
        {
            _authManager = authManager;
            _logger = logger;
            _repository = repository;
        }

        public override IActionResult OnGet()
        {
            RoleModel = new RoleModel()
                .InitRoleClaims(_authManager);

            return Page();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        public override async Task<IActionResult> OnPostAsync(string hfClaimList)
        {
            RoleModel.InitRoleClaims(_authManager)
                .SetAssignedClaims(
                    hfClaimList?.Split(',') ?? Array.Empty<string>()
                    );

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var role = CreateRole(RoleModel);

            var result = await _authManager.AuthorizeAsync(User, role, new AppClaimRequirement(SysClaims.Role.Create));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can not create Role:");
                ModelState.AddModelError(string.Empty, "You can not create a Role with more privileges than you have.");

                _logger.LogInformation($"User '{User.Identity.Name}' (ID '{User.UserId()}') attempted to create {typeof(TRole).Name} with elevated privileges.");

                return Page();
            }

            try
            {
                _repository.Roles.Add(role);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Could not create Role:");
                ModelState.AddModelError(string.Empty, ex.GetBaseException().Message);

                _logger.LogError(ex, $"Could not create {typeof(TRole).Name} '{role.Name}' (ID '{role.Id}').");

                return Page();
            }

            SendNotification(
                typeof(IndexModel), Severity.Normal,
                $"Role '{role.Name}' successfully created."
                );

            _logger.LogInformation($"{typeof(TRole).Name} '{role.Name}' (ID '{role.Id}') was created.");

            return RedirectToPage(IndexModel.PageName);

        }

        private static TRole CreateRole(RoleModel model)
        {
            var normalizer = new UpperInvariantLookupNormalizer();

            var role = new TRole
            {
                Description = model.Description,
                Name = model.Name,
                NormalizedName = normalizer.NormalizeName(model.Name)
            }.SetClaims(model.GetAssignedClaims());

            return role;
        }
    }
}
