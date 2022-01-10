using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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
                logger.LogError(ex, "Database.MigrateAsync() failed.");
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
