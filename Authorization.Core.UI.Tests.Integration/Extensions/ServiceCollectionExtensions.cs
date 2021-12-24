using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Authorization.Core.UI.Tests.Integration.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection SetupTestDatabase<TContext>(this IServiceCollection services, DbConnection connection) where TContext : DbContext
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddScoped(serviceProvider =>
                DbContextOptionsFactory<TContext>(
                    serviceProvider,
                    (sp, options) => options
                        .ConfigureWarnings(b => b.Log(CoreEventId.ManyServiceProvidersCreatedWarning))
                        .UseSqlite(connection)
                        ));

            return services;
        }

        private static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            IServiceProvider applicationServiceProvider,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var builder = new DbContextOptionsBuilder<TContext>(
                new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>()));

            builder.UseApplicationServiceProvider(applicationServiceProvider);

            optionsAction?.Invoke(applicationServiceProvider, builder);

            return builder.Options;
        }
    }
}
