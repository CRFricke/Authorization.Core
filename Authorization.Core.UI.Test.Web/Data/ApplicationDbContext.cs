using Fricke.Authorization.Core.UI.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.UI.Test.Web.Data
{
    public class ApplicationDbContext : AppDbContext<AppUser, AppRole>
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
    }
}
