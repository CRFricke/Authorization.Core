using CRFricke.Authorization.Core;
using CRFricke.Authorization.Core.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.UI.Test.Web.Data
{
    public class ApplicationDbContext : AuthUiContext<AuthUiUser, AuthUiRole>
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
        public override void SeedDatabase()
        {
            base.SeedDatabase();

            var normalizer = new UpperInvariantLookupNormalizer();

            var role = Roles.Find(AppGuids.Role.CalendarManager);
            if (role == null)
            {
                role = new AuthUiRole
                {
                    Id = AppGuids.Role.CalendarManager,
                    Name = nameof(AppGuids.Role.CalendarManager),
                    Description = "CalendarManagers are responsible for managing the company's calendar.",
                    NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.CalendarManager))
                }.SetClaims(AppClaims.Calendar.DefinedClaims);

                Roles.Add(role);
            }

            var user = Users.Find(AppGuids.User.CalendarGuy);
            if (user == null)
            {
                var email = "CalendarGuy@company.com";

                user = new AuthUiUser
                {
                    Id = AppGuids.User.CalendarGuy,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Julian",
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEFXwSRmwaiwTjRDW4zcaupENlSdbverXypglebb + Ti6f / Rn4sBikU3q / uE0jJQJAMw ==", // "Calend@rGuy!"
                    Surname = "Day",
                    UserName = email
                }.SetClaims(new[] { role.Name });

                Users.Add(user);
            }

            role = Roles.Find(AppGuids.Role.DocumentManager);
            if (role == null)
            {
                role = new AuthUiRole
                {
                    Id = AppGuids.Role.DocumentManager,
                    Name = nameof(AppGuids.Role.DocumentManager),
                    Description = "DocumentManagers are responsible for managing the company's documents.",
                    NormalizedName = normalizer.NormalizeName(nameof(AppGuids.Role.DocumentManager))
                }.SetClaims(AppClaims.Document.DefinedClaims);

                Roles.Add(role);
            }

            user = Users.Find(AppGuids.User.DocumentGuy);
            if (user == null)
            {
                var email = "DocumentGuy@company.com";

                user = new AuthUiUser
                {
                    Id = AppGuids.User.DocumentGuy,
                    Email = email,
                    EmailConfirmed = true,
                    GivenName = "Mark",
                    LockoutEnabled = false,
                    NormalizedEmail = normalizer.NormalizeEmail(email),
                    NormalizedUserName = normalizer.NormalizeName(email),
                    PasswordHash = "AQAAAAEAACcQAAAAEJaNzNSqF3SrSxUHuT010YO6kAmf95+Xv20mzd3MzLBTNU8ySBGMBqkx82q3Be+BCg==", // "D0cumentGuy!"
                    Surname = "Hofmann",
                    UserName = email
                }.SetClaims(new[] { role.Name });

                Users.Add(user);
            }

            SaveChanges();
        }
    }
}
