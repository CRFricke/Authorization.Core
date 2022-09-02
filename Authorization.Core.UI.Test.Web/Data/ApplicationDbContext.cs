using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Authorization.Core.UI.Test.Web.Data
{
    public class ApplicationDbContext : AuthUiContext<ApplicationUser, ApplicationRole>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class using default options.
        /// </summary>
        public ApplicationDbContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class using the specified options.
        /// </summary>
        /// <param name="options">The options to be used by the new <see cref="ApplicationDbContext"/> instance.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }


        /// <inheritdoc/>
        public override async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            await base.SeedDatabaseAsync(serviceProvider);

            var normalizer = serviceProvider.GetRequiredService<ILookupNormalizer>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ApplicationDbContext>();

            var role = await Roles.FindAsync(AppGuids.Role.CalendarManager);
            if (role == null)
            {
                role = new ApplicationRole
                {
                    Id = AppGuids.Role.CalendarManager,
                    Name = nameof(AppGuids.Role.CalendarManager),
                    Description = "CalendarManagers are responsible for managing the company's calendar.",
                    NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.CalendarManager))
                }.SetClaims(AppClaims.Calendar.DefinedClaims);

                await Roles.AddAsync(role);
                logger.LogInformation(
                    "{RoleType} '{RoleName}' (ID: {RoleId}) has been created.",
                    nameof(ApplicationRole), role.Name, role.Id
                    );
            }

            var user = await Users.FindAsync(AppGuids.User.CalendarGuy);
            if (user == null)
            {
                var email = "CalendarGuy@company.com";

                user = new ApplicationUser
                {
                    Id = AppGuids.User.CalendarGuy,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Calendar",
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEFXwSRmwaiwTjRDW4zcaupENlSdbverXypglebb + Ti6f / Rn4sBikU3q / uE0jJQJAMw ==", // "Calend@rGuy!"
                    Surname = "Guy",
                    UserName = email
                }.SetClaims(new[] { role.Name });

                await Users.AddAsync(user);
                logger.LogInformation(
                    "{UserType} '{UserEmail}' (ID: {UserId}) has been created.",
                    nameof(ApplicationUser), user.Email, user.Id
                    );
            }

            role = await Roles.FindAsync(AppGuids.Role.DocumentManager);
            if (role == null)
            {
                role = new ApplicationRole
                {
                    Id = AppGuids.Role.DocumentManager,
                    Name = nameof(AppGuids.Role.DocumentManager),
                    Description = "DocumentManagers are responsible for managing the company's documents.",
                    NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.DocumentManager))
                }.SetClaims(AppClaims.Document.DefinedClaims);

                await Roles.AddAsync(role);
                logger.LogInformation(
                    "{RoleType} '{RoleName}' (ID: {RoleId}) has been created.",
                    nameof(ApplicationRole), role.Name, role.Id
                    );
            }

            user = await Users.FindAsync(AppGuids.User.DocumentGuy);
            if (user == null)
            {
                var email = "DocumentGuy@company.com";

                user = new ApplicationUser
                {
                    Id = AppGuids.User.DocumentGuy,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Document",
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEJaNzNSqF3SrSxUHuT010YO6kAmf95+Xv20mzd3MzLBTNU8ySBGMBqkx82q3Be+BCg==", // "D0cumentGuy!"
                    Surname = "Guy",
                    UserName = email
                }.SetClaims(new[] { role.Name });

                await Users.AddAsync(user);
                logger.LogInformation(
                    "{UserType} '{UserEmail}' (ID: {UserId}) has been created.",
                    nameof(ApplicationUser), user.Email, user.Id
                    );
            }

            try
            {
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SaveChangesAsync() method failed.");
            }
        }
    }
}
