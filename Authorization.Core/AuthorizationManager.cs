using CRFricke.Authorization.Core.Attributes;
using CRFricke.Authorization.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Extends ASP.NET Core Identity to provide authorization based on entity CRUD permission claims
    /// (e.g. "User.Create", "User.Read", "User.Update", "User.Delete", and "User.List").
    /// </summary>
    /// <typeparam name="TUser">The <see cref="Type"/> of user objects. The Type must be or extend from <see cref="AuthUser"/>.</typeparam>
    /// <typeparam name="TRole">The <see cref="Type"/> of role objects. The Type must be or extend from <see cref="AuthRole"/>.</typeparam>
    public sealed class AuthorizationManager<TUser, TRole> : IAuthorizationManager
        where TUser : AuthUser
        where TRole : AuthRole
    {
        /// <summary>
        /// Initializes the static properties of the class.
        /// </summary>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "GetReferencedAssemblies() called on assemblies contained in AppDomain.CurrentDomain.")]
        static AuthorizationManager()
        {
            Assembly thisAssembly = typeof(AuthorizationManager<TUser, TRole>).Assembly;
            string assemblyName = thisAssembly.GetName().Name!;

            List<Assembly> assemblies = new() { thisAssembly };
            assemblies.AddRange(
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)   // Ignore Mocked assemblies during testing (GetExportedTypes() throws exception)
                    .Where(a => a.GetReferencedAssemblies().Any(an => an.Name == assemblyName))
                );

            LoadSystemClaims(assemblies);
            LoadSystemGuids(assemblies);
        }

        /// <summary>
        /// Loads the Claims that are installed by the application.
        /// </summary>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "assembly.GetExportedTypes(): assembly instances loaded from AppDomain.CurrentDomain.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2111:DynamicallyAccessedMembers", Justification = "Activator.CreateInstance(): 'IDefinesClaims' implementation classes always have a public parameter-less constructor.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:DynamicallyAccessedMembers", Justification = "claimClass.GetType().GetFields(): claimClass instances are loaded prior to call.")]
        private static void LoadSystemClaims(List<Assembly> assemblies)
        {
            List<IDefinesClaims> claimClasses = new();

            // Load all classes that implement IDefinesClaims interface.
            foreach (Assembly assembly in assemblies)
            {
                claimClasses.AddRange(
                    assembly.GetExportedTypes()
                        .Where(t => typeof(IDefinesClaims).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .Select(Activator.CreateInstance).Cast<IDefinesClaims>()
                    );
            }

            // Now retrieve the Claims defined in each class
            foreach (var claimClass in claimClasses)
            {
                DefinedClaims.AddRange(claimClass.DefinedClaims);

                // Save any claims that are marked with the RestrictedClaim attribute
                RestrictedClaims.AddRange(
                    from FieldInfo fi in claimClass.GetType().GetFields()
                    where Attribute.IsDefined(fi, typeof(RestrictedClaimAttribute))
                    let claim = fi.GetRawConstantValue() as string
                    where claim != null
                    select claim
                    );
            }
        }

        /// <summary>
        /// Loads the GUIDs of the entities installed by the application.
        /// </summary>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "assembly.GetExportedTypes(): assembly instances loaded from AppDomain.CurrentDomain.")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2111:DynamicallyAccessedMembers", Justification = "Activator.CreateInstance(): 'IDefinesClaims' implementation classes always have a public parameter-less constructor.")]
        private static void LoadSystemGuids(List<Assembly> assemblies)
        {
            List<IDefinesGuids> guidClasses = new();

            // Load all classes that implement IDefinesGuids interface.
            foreach (Assembly assembly in assemblies)
            {
                guidClasses.AddRange(
                    assembly.GetExportedTypes()
                        .Where(t => typeof(IDefinesGuids).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .Select(Activator.CreateInstance).Cast<IDefinesGuids>()
                    );
            }

            // Now load the GUIDs defined in each class
            foreach (var guidClass in guidClasses)
            {
                DefinedGuids.AddRange(guidClass.DefinedGuids);
            }
        }


        /// <summary>
        /// Contains the Claims that are restricted by the application.
        /// </summary>
        private static List<string> RestrictedClaims { get; } = new List<string>();

        /// <summary>
        /// Contains the application Claims installed by the application.
        /// </summary>
        private static List<string> DefinedClaims { get; } = new List<string>();

        /// <summary>
        /// Contains the GUIDs of the entities installed by the application.
        /// </summary>
        private static List<string> DefinedGuids { get; } = new List<string>();


        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthorizationManager> _logger;


        /// <summary>
        /// Internal constructor for testing framework.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal AuthorizationManager()
        { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Creates a new instance of the AuthorizationManager class using the specified parameters.
        /// </summary>
        /// <param name="serviceProvider">The ServiceProvider to be used to access required services.</param>
        /// <param name="logger">The logger to be used for logging.</param>
        public AuthorizationManager(IServiceProvider serviceProvider, ILogger<AuthorizationManager> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Returns a list of all claims defined by the application.
        /// </summary>
        List<string> IAuthorizationManager.DefinedClaims => DefinedClaims;

        /// <summary>
        /// Returns a list of all Guids defined by the application.
        /// </summary>
        List<string> IAuthorizationManager.DefinedGuids => DefinedGuids;



        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/> 
        /// for the specified <paramref name="resource"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the authenticated user.</param>
        /// <param name="resource">The resource to be tested against.</param>
        /// <param name="claimRequirement">The requirement that must be met for authorization to be granted.</param>
        /// <returns>
        /// An <see cref="AuthorizationResult"/> specifying whether the <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </returns>
        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, object resource, AppClaimRequirement claimRequirement)
        {
            if (resource == null)
            {
                return await AuthorizeAsync(principal, claimRequirement);
            }

            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (claimRequirement == null) throw new ArgumentNullException(nameof(claimRequirement));
            if (resource is not IRequiresAuthorization raObject)
                throw new ArgumentException($"Argument does not implement {nameof(IRequiresAuthorization)} interface.", nameof(resource));

            var principalId = principal.UserId();
            if (principalId == null)
            {
                _logger.LogDebug(
                    "{ClassName} for \"{AppClaimRequirement}\" not met for user '{UserName}' - user ID is null.",
                    nameof(AppClaimRequirement), claimRequirement, principal.UserName()
                    );
                return AuthorizationResult.NoUserId(claimRequirement.ClaimValues);
            }

            // Is this a system User or Role?
            if (DefinedGuids.Contains(raObject.Id))
            {
                // Yes - fail requests for restricted claims
                var failedClaims = claimRequirement.ClaimValues.Intersect(RestrictedClaims);
                if (failedClaims.Any())
                {
                    var userName = principal.UserName();
                    var resourceType = resource.GetType().Name;
                    var failedClaim = failedClaims.First();

                    _logger.LogInformation(
                        "{ClassName} of \"{Claim}\" for {ResourceType} '{ObjectName}' not met by '{UserName}' - restricted operation on system User or Role.",
                        nameof(AppClaimRequirement), failedClaim, resourceType, raObject.Name, userName
                        );
                    return AuthorizationResult.SystemObject(failedClaims);
                }
            }

            var result = await AuthorizeInternalAsync(principal, claimRequirement);
            if (!result.Succeeded)
            {
                return result;
            }

            if (resource is TRole role && !claimRequirement.ClaimValues.Contains(SysClaims.Role.Delete))
            {
                return await ElevationCheckAsync(principalId, role);
            }

            if (resource is TUser user && !claimRequirement.ClaimValues.Contains(SysClaims.User.Delete))
            {
                return await ElevationCheckAsync(principalId, user);
            }

            return result;
        }

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the authenticated user.</param>
        /// <param name="claimRequirement">The requirement that must be met for authorization to be granted.</param>
        /// <returns>
        /// An <see cref="AuthorizationResult"/> specifying whether the <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </returns>
        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal principal, AppClaimRequirement claimRequirement)
        {
            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (claimRequirement == null) throw new ArgumentNullException(nameof(claimRequirement));

            var userId = principal.UserId();
            if (userId == null)
            {
                _logger.LogDebug(
                    "{ClassName} for \"{AppClaimRequirement}\" not met for user '{UserName}' - user ID is null.",
                    nameof(AppClaimRequirement), claimRequirement, principal.UserName()
                    );
                return AuthorizationResult.NoUserId(claimRequirement.ClaimValues);
            }

            return await AuthorizeInternalAsync(principal, claimRequirement);
        }

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> representing the authenticated user.</param>
        /// <param name="claimRequirement">The requirement that must be met for authorization to be granted.</param>
        /// <returns>
        /// An <see cref="AuthorizationResult"/> specifying whether the <paramref name="principal"/> meets the specified <paramref name="claimRequirement"/>.
        /// </returns>
        private async Task<AuthorizationResult> AuthorizeInternalAsync(ClaimsPrincipal principal, AppClaimRequirement claimRequirement)
        {
            var userId = principal.UserId() ?? throw new InvalidOperationException("Specified ClaimsPrincipal does not have a 'NameIdentifier' Claim.");

            var principalRoles = await GetUserRolesAsync(userId);
            if (principalRoles.Contains(SysGuids.Role.Administrator))
            {
                _logger.LogDebug(
                    "{ClassName} for \"{AppClaimRequirement}\" met for user '{UserName}' via {RoleName} role.",
                    nameof(AppClaimRequirement), claimRequirement, principal.UserName(), nameof(SysGuids.Role.Administrator)
                    );
                return AuthorizationResult.Success();
            }

            HashSet<string> principalClaims = await GetRoleClaimsAsync(principalRoles);
            if (claimRequirement.ClaimValues.IsSubsetOf(principalClaims))
            {
                _logger.LogDebug(
                    "{ClassName} for \"{AppClaimRequirement}\" met for user '{UserName}'.",
                    nameof(AppClaimRequirement), claimRequirement, principal.UserName()
                    );
                return AuthorizationResult.Success();
            }

            return AuthorizationResult.Failed(
                claimRequirement.ClaimValues.Except(principalClaims)
                );
        }

        /// <summary>
        /// Determines whether the supplied User object has more privileges than the logged in user has.
        /// </summary>
        /// <param name="principalId">The ID of the logged in user.</param>
        /// <param name="user">The User object to be checked.</param>
        /// <returns>An <see cref="AuthorizationResult"/> object that indicates the results of the check.</returns>
        private async Task<AuthorizationResult> ElevationCheckAsync(string principalId, TUser user)
        {
            var principalRoles = await GetUserRolesAsync(principalId);
            if (principalRoles.Contains(SysGuids.Role.Administrator))
            {
                return AuthorizationResult.Success();
            }

            var userRoles = await GetUserRolesAsync(user);
            if (userRoles.Contains(SysGuids.Role.Administrator))
            {
                return AuthorizationResult.Elevation(new[] { nameof(SysGuids.Role.Administrator) });
            }

            var principalClaims = await GetRoleClaimsAsync(principalRoles);
            var userClaims = await GetRoleClaimsAsync(userRoles);

            if (userClaims.IsSubsetOf(principalClaims))
            {
                return AuthorizationResult.Success();
            };

            return AuthorizationResult.Elevation(
                userClaims.Except(principalClaims)
                );
        }

        /// <summary>
        /// Determines whether the supplied Role object has more privileges than the logged in user has.
        /// </summary>
        /// <param name="principalId">The ID of the logged in user.</param>
        /// <param name="role">The Role object to be checked.</param>
        /// <returns>An <see cref="AuthorizationResult"/> object that indicates the results of the check.</returns>
        private async Task<AuthorizationResult> ElevationCheckAsync(string principalId, TRole role)
        {
            var principalRoles = await GetUserRolesAsync(principalId);
            if (principalRoles.Contains(SysGuids.Role.Administrator))
            {
                return AuthorizationResult.Success();
            }

            var principalClaims = await GetRoleClaimsAsync(principalRoles);
            var roleClaims = AuthorizationManager<TUser, TRole>.GetRoleClaims(role);

            if (roleClaims.IsSubsetOf(principalClaims))
            {
                return AuthorizationResult.Success();
            }

            return AuthorizationResult.Elevation(
                roleClaims.Except(principalClaims)
                );
        }

        /// <summary>
        /// Returns the claims associated with the specified Role.
        /// </summary>
        /// <param name="role">The Role whose claims are to be retrieved.</param>
        /// <returns>A HashSet containing the IDs of the associated claims.</returns>
        private static HashSet<string> GetRoleClaims(TRole role)
        {
            return (
                from ar in role.Claims
                where ar.ClaimType == SysClaims.ClaimType
                select ar.ClaimValue
                ).ToHashSet();
        }

        /// <summary>
        /// Returns the claims associated with the specified Roles.
        /// </summary>
        /// <param name="roleIds">A HashSet containing the IDs of the Roles whose claims are to be retrieved.</param>
        /// <returns>A HashSet containing the IDs of the associated claims.</returns>
        private async Task<HashSet<string>> GetRoleClaimsAsync(HashSet<string> roleIds)
        {
            var claimHash = new HashSet<string>();

            foreach (var roleId in roleIds)
            {
                claimHash.UnionWith(await GetRoleClaimsAsync(roleId));
            }

            return claimHash;
        }

        /// <summary>
        /// Returns the claims associated with the specified Role.
        /// </summary>
        /// <param name="roleId">The ID of the Role whose claims are to be retrieved.</param>
        /// <returns>A HashSet containing the IDs of the associated claims.</returns>
        private async Task<HashSet<string>> GetRoleClaimsAsync(string roleId)
        {
            var roleClaimCache = _serviceProvider.GetRequiredService<RoleClaimCache>();

            if (!roleClaimCache.TryGetValue(roleId, out HashSet<string>? hashSet))
            {
                var dbContext = (IRepository<TUser, TRole>)
                    _serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext.RequestServices.GetRequiredService(typeof(IRepository<TUser, TRole>));

                hashSet = (await
                    dbContext.Set<IdentityRoleClaim<string>>()
                    .Where(rc => rc.RoleId == roleId && rc.ClaimType == SysClaims.ClaimType)
                    .Select(rc => rc.ClaimValue!)
                    .ToArrayAsync()
                    ).ToHashSet();

                roleClaimCache.Set(
                    roleId, hashSet,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3))
                    );
            }

            return hashSet!;
        }

        /// <summary>
        /// Returns the IDs of the Roles assigned to the specified user.
        /// </summary>
        /// <param name="user">The user whose Role IDs are to be returned.</param>
        /// <returns>A HashSet containing the IDs of the assigned roles.</returns>
        private async Task<HashSet<string>> GetUserRolesAsync(TUser user)
        {
            var dbContext = (IRepository<TUser, TRole>)
                _serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext.RequestServices.GetRequiredService(typeof(IRepository<TUser, TRole>));

            var roleNames =
                from uc in user.Claims
                where uc.UserId == user.Id && uc.ClaimType == ClaimTypes.Role
                select uc.ClaimValue;

            return (await (
                from ar in dbContext.Set<TRole>()
                where roleNames.Contains(ar.Name)
                select ar.Id
                ).ToArrayAsync()).ToHashSet();
        }

        /// <summary>
        /// Returns the IDs of any Roles assigned to the specified user.
        /// </summary>
        /// <param name="userId">The Id of the user whose Role IDs are to be returned.</param>
        /// <returns>A HashSet containing the IDs of the assigned roles.</returns>
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "Initialized dbContext instance loaded from IServiceProvider before LINQ query.")]
        private async Task<HashSet<string>> GetUserRolesAsync(string userId)
        {
            var userRoleCache = _serviceProvider.GetRequiredService<UserRoleCache>();

            if (!userRoleCache.TryGetValue(userId, out HashSet<string>? hashSet))
            {
                var dbContext = (IRepository<TUser, TRole>)
                    _serviceProvider.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext.RequestServices.GetRequiredService(typeof(IRepository<TUser, TRole>));

                hashSet = (await (
                    from uc in dbContext.Set<IdentityUserClaim<string>>()
                    join ar in dbContext.Set<TRole>() on uc.ClaimValue equals ar.Name
                    where uc.UserId == userId && uc.ClaimType == ClaimTypes.Role
                    select ar.Id
                    ).ToArrayAsync()).ToHashSet();

                userRoleCache.Set(
                    userId, hashSet,
                    new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3))
                    );
            }

            return hashSet!;
        }

        /// <summary>
        /// Returns an indication of whether the specified <paramref name="principal"/> has the specified <paramref name="requiredClaims"/>. 
        /// </summary>
        /// <param name="principal">A <see cref="ClaimsPrincipal"/> representing the user.</param>
        /// <param name="requiredClaims">The claims that the <paramref name="principal"/> must have for authorization to be granted.</param>
        /// <returns>
        /// <em>true</em>, if the specified <paramref name="principal"/> has the specified <paramref name="requiredClaims"/>; otherwise, <em>false</em>.
        /// </returns>
        public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal principal, params string[] requiredClaims)
        {
            var result = await AuthorizeAsync(principal, new AppClaimRequirement(requiredClaims));
            return result.Succeeded;
        }

        /// <summary>
        /// Removes the RoleClaimCache entry of the specified role.
        /// </summary>
        /// <param name="roleId">The ID of the Role whose RoleClaimCache entry is to be removed.</param>
        public void RefreshRole(string roleId)
        {
            _serviceProvider.GetRequiredService<RoleClaimCache>()
                .Remove(roleId);
        }

        /// <summary>
        /// Removes the UserRoleCache entry of the specified user.
        /// </summary>
        /// <param name="userId">The ID of the User whose UserRoleCache entry is to be removed.</param>
        public void RefreshUser(string userId)
        {
            _serviceProvider.GetRequiredService<UserRoleCache>()
                .Remove(userId);
        }
    }

    /// <summary>
    /// Dummy class used for logging.
    /// </summary>
    public class AuthorizationManager
    { }
}
