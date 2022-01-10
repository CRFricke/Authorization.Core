using CRFricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.UI.Tests.Data
{
    public class ApplicationDbContext : AuthUiContext<ApplicationUser, ApplicationRole>
    {
        /// <summary>
        /// Used for testing.
        /// </summary>
        public ApplicationDbContext()
        { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
