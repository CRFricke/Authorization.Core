using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Fricke.Authorization.Core.UI
{
    internal class ConfigureAuthCoreUIRazorOptions<TUser, TRole> : IPostConfigureOptions<RazorPagesOptions>
        where TUser : class
        where TRole : class
    {
        public ConfigureAuthCoreUIRazorOptions(IOptions<AuthCoreUIOptions> options)
        {
            _uiOptions = options.Value;
        }

        private const string AuthCoreUIAreaName = "Authorization";
        private readonly AuthCoreUIOptions _uiOptions;

        public void PostConfigure(string name, RazorPagesOptions options)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            options = options ?? throw new ArgumentNullException(nameof(options));

            var friendlyAreaName = _uiOptions.FriendlyAreaName;

            if (!string.IsNullOrEmpty(_uiOptions.FriendlyAreaName))
            {
                options.Conventions
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Index", $"{friendlyAreaName}/Role")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Create", $"{friendlyAreaName}/Role/Create")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Delete", $"{friendlyAreaName}/Role/Delete")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Details", $"{friendlyAreaName}/Role/Details")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Edit", $"{friendlyAreaName}/Role/Edit")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/Role/Index", $"{friendlyAreaName}/Role/Index")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Index", $"{friendlyAreaName}/User")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Create", $"{friendlyAreaName}/User/Create")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Delete", $"{friendlyAreaName}/User/Delete")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Details", $"{friendlyAreaName}/User/Details")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Edit", $"{friendlyAreaName}/User/Edit")
                    .AddAreaPageRoute(AuthCoreUIAreaName, "/User/Index", $"{friendlyAreaName}/User/Index");
            }

            var convention = new AuthCorePageApplicationModelConvention<TUser, TRole>();
            options.Conventions.AddAreaFolderApplicationModelConvention(
                AuthCoreUIAreaName,
                "/",
                pam => convention.Apply(pam));
        }
    }
}