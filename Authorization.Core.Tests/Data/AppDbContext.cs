using Fricke.Authorization.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.Tests.Data
{
    public class AppDbContext : AuthDbContext<AppUser, AppRole>
    {
        /// <summary>
        /// Used for testing.
        /// </summary>
        public AppDbContext()
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
