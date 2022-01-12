using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CRFricke.Authorization.Core.UI
{
    public static class IdentityBuilderExtensions
    {
        public static IdentityBuilder AddCRFrickeAuthorizationCoreUI(this IdentityBuilder builder)
        {
            return AddCRFrickeAuthorizationCoreUI(builder, o => { });
        }

        public static IdentityBuilder AddCRFrickeAuthorizationCoreUI(this IdentityBuilder builder, Action<AuthCoreUIOptions> configUIOptions)
        {
            var roleType = builder.RoleType ?? typeof(IdentityUserRole<string>);

            builder.Services.Configure(configUIOptions);

            builder.Services.ConfigureOptions(typeof(ConfigureAuthCoreUIRazorOptions<,>)
                .MakeGenericType(builder.UserType, roleType));

            return builder;
        }
    }
}
