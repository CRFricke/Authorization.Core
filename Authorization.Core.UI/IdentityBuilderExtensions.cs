using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CRFricke.Authorization.Core.UI
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddAuthorizationCoreUI(this IdentityBuilder builder)
        {
            return AddAuthorizationCoreUI(builder, o => { });
        }

        public static IdentityBuilder AddAuthorizationCoreUI(this IdentityBuilder builder, Action<AuthCoreUIOptions> configUIOptions)
        {
            var roleType = builder.RoleType ?? typeof(IdentityUserRole<string>);

            builder.Services.Configure(configUIOptions);

            builder.Services.ConfigureOptions(
                typeof(ConfigureAuthCoreUIRazorOptions<,>).MakeGenericType(builder.UserType, roleType)
                );

            return builder;
        }
    }
}
