using CRFricke.Authorization.Core.Data;
using CRFricke.EF.Core.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="IdentityBuilder"/> class.
    /// </summary>
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds AccessRight based authorization services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
        /// <param name="dbInitializationOption">
        /// The <see cref="DbInitializationOption"/> to be used to initialize the database.
        /// </param>
        /// <returns>The <see cref="IdentityBuilder"/> instance this method extends.</returns>
        public static IdentityBuilder AddAccessRightBasedAuthorization(this IdentityBuilder builder, DbInitializationOption dbInitializationOption = DbInitializationOption.Migrate)
        {
            Type contextType;

            using (var serviceProvider = builder.Services.BuildServiceProvider())
            {
                var storeType = builder.RoleType != null
                    ? serviceProvider.GetRequiredService(typeof(IRoleStore<>).MakeGenericType(builder.RoleType)).GetType()
                    : serviceProvider.GetRequiredService(typeof(IUserStore<>).MakeGenericType(builder.UserType)).GetType();

                contextType = storeType.GenericTypeArguments[1];
            }

            VerifyTypeDerivesFrom(contextType, typeof(AuthDbContext<,>));

            VerifyTypeDerivesFrom(builder.UserType, typeof(AuthUser));

            if (builder.RoleType != null)
            {
                VerifyTypeDerivesFrom(builder.RoleType, typeof(AuthRole));
            }

            var roleType = builder.RoleType ?? typeof(AuthRole);

            var authManagerType = typeof(AuthorizationManager<,>)
                .MakeGenericType(builder.UserType, roleType);
            var iRepository = typeof(IRepository<,>)
                .MakeGenericType(builder.UserType, roleType);

            builder.Services
                .AddHttpContextAccessor()
                .AddSingleton<RoleClaimCache>()
                .AddSingleton<UserRoleCache>()
                .AddScoped(iRepository, contextType)
                .AddSingleton(typeof(IAuthorizationManager), authManagerType)
                .AddSingleton<IAuthorizationPolicyProvider, AppClaimRequirementProvider>()
                .AddSingleton<IAuthorizationHandler, AppClaimRequirementHandler>()
                .AddDbInitializer(options =>
                    options.UseDbContext(contextType, dbInitializationOption)
                    );

            return builder;
        }

        private static void VerifyTypeDerivesFrom(Type type, Type baseType)
        {
            if (!TypeDerivesFrom(type, baseType))
            {
                throw new InvalidOperationException(
                    $"'{type}' is not derived from '{baseType}' class."
                    );
            }
        }


        /// <summary>
        /// Determines whether a specified <see cref="Type"/> derives from the specified base Type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to be checked.</param>
        /// <param name="baseType">The base <see cref="Type"/> to checked for.</param>
        /// <returns>
        /// <em>true</em>, if the <see cref="Type"/> derives from the base Type; otherwise, <em>false</em>.
        /// </returns>
        private static bool TypeDerivesFrom(Type? type, Type baseType)
        {
            while (type != null)
            {
                if (type.Name == baseType.Name)
                {
                    if (type.GenericTypeArguments.Length == baseType.GenericTypeArguments.Length)
                    {
                        for (var ix = 0; ix < type.GenericTypeArguments.Length; ix++)
                        {
                            if (!baseType.GenericTypeArguments[ix].IsAssignableFrom(type.GenericTypeArguments[ix]))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                type = type.BaseType;
            };

            return false;
        }
    }
}
