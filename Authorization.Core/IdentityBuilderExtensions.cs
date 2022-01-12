using CRFricke.Authorization.Core.Data;
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
        /// Adds the Authorization.Core services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TContext">A DB Context class that derives from <see cref="AuthDbContext"/></typeparam>
        /// <param name="builder">The <see cref="IdentityBuilder"/> object.</param>
        public static IdentityBuilder AddCRFrickeAuthorizationCore<TContext>(this IdentityBuilder builder)
        {
            var roleType = builder.RoleType ?? typeof(AuthRole);

            VerifyTypeDerivesFrom(typeof(TContext), typeof(AuthDbContext<,>));
            VerifyTypeDerivesFrom(builder.UserType, typeof(AuthUser));
            VerifyTypeDerivesFrom(roleType, typeof(AuthRole));

            var authManagerType = typeof(AuthorizationManager<,>)
                .MakeGenericType(builder.UserType, roleType);
            var iRepository = typeof(IRepository<,>)
                .MakeGenericType(builder.UserType, roleType);

            builder.Services
                .AddHttpContextAccessor()
                .AddSingleton<RoleClaimCache>()
                .AddSingleton<UserRoleCache>()
                .AddScoped(iRepository, typeof(TContext))
                .AddSingleton(typeof(IAuthorizationManager), authManagerType)
                .AddSingleton<IAuthorizationPolicyProvider, AppClaimRequirementProvider>()
                .AddSingleton<IAuthorizationHandler, AppClaimRequirementHandler>();

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
        private static bool TypeDerivesFrom(Type type, Type baseType)
        {
            do
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
            } while (type != null);

            return false;
        }
    }
}
