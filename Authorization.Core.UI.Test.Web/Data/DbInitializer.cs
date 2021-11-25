using Authorization.Core.UI.Tests.Web.Authorization;
using Fricke.Authorization.Core;
using Fricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization.Core.UI.Test.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDatabaseAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database.MigrateAsync() call failed.");
                throw;
            }

            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            var cgEmail = "CalendarGuy@company.com";
            var dgEmail = "DocumentGuy@company.com";

            var role = new AppRole
            {
                Id = AppGuids.Role.CalendarManager,
                Name = nameof(AppGuids.Role.CalendarManager),
                Description = "CalendarManagers are responsible for managing the company's calendar."
            }.SetClaims(AppClaims.Calendar.DefinedClaims);

            await CreateRoleAsync(roleManager, logger, role);

            var user = new AppUser()
            {
                Id = AppGuids.User.CalendarGuy,
                Email = cgEmail,
                EmailConfirmed = true,
                LockoutEnabled = false,
                UserName = cgEmail,
                GivenName = "Julian",
                Surname = "Day"
            }.SetClaims(new[] { role.Name });

            await CreateUserAsync(userManager, logger, user, "D!aP1Q1gBZQQ");

            role = new AppRole
            {
                Id = AppGuids.Role.DocumentManager,
                Name = nameof(AppGuids.Role.DocumentManager),
                Description = "DocumentManagers are responsible for managing the company's documents."
            }.SetClaims(AppClaims.Document.DefinedClaims);

            await CreateRoleAsync(roleManager, logger, role);

            user = new AppUser()
            {
                Id = AppGuids.User.DocumentGuy,
                Email = dgEmail,
                EmailConfirmed = true,
                LockoutEnabled = false,
                UserName = dgEmail,
                GivenName = "Mark",
                Surname = "Hofmann"
            }.SetClaims(new[] { role.Name });

            await CreateUserAsync(userManager, logger, user, "jnNrF!x9T5BC");
        }

        private static async Task<AppRole> CreateRoleAsync(RoleManager<AppRole> roleManager, ILogger logger, AppRole role)
        {
            var duplicateRole = await roleManager.FindByIdAsync(role.Id);
            if (duplicateRole != null)
            {
                return duplicateRole;
            }

            var result = await roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                LogError(logger, role, result.Errors);
            }

            logger.LogInformation($"{nameof(AppRole)} '{role.Name}' successfully created.");
            return role;
        }

        private static async Task<AppUser> CreateUserAsync(UserManager<AppUser> userManager, ILogger logger, AppUser user, string password)
        {
            var duplicateUser = await userManager.FindByIdAsync(user.Id);
            if (duplicateUser != null)
            {
                return duplicateUser;
            }
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                LogError(logger, user, result.Errors);
            }

            logger.LogInformation($"{nameof(AppUser)} '{user.Email}' successfully created.");
            return user;
        }

        private static void LogError(ILogger logger, AppRole role, IEnumerable<IdentityError> errors)
        {
            LogError(logger, $"Could not create '{role.Name}' role.", errors);
        }

        private static void LogError(ILogger logger, AppUser user, IEnumerable<IdentityError> errors)
        {
            LogError(logger, $"Could not create '{user.Email}' user.", errors);
        }

        private static void LogError(ILogger logger, string message, IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                logger.LogError($"{message}: {error}");
            }
        }
    }
}
