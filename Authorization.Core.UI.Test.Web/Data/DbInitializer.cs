using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Authorization.Core.UI.Test.Web.Data
{
    public static class DbInitializer
    {
        public static void InitializeDatabase(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));
            var dbContext = services.GetRequiredService<ApplicationDbContext>();

            try
            {
                dbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database.Migrate() failed.");
                throw;
            }

            try
            {
                dbContext.SeedDatabase();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SeedDatabase() failed.");
                throw;
            }
        }
    }
}
