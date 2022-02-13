using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace CRFricke.Authorization.Core.UI
{
    public static class IdentityBuilderExtensions
    {
        /// <summary>
        /// Adds a default, self contained, UI for managing the User and Role entities exposed by the Authorization.Core package.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/>.</param>
        /// <returns>The same <see cref="IdentityBuilder"/> so multiple calls can be chained.</returns>
        public static IdentityBuilder AddAuthorizationCoreUI(this IdentityBuilder builder)
        {
            return AddAuthorizationCoreUI(builder, o => { });
        }

        /// <summary>
        /// Adds a default, self contained, UI for managing the User and Role entities exposed by the Authorization.Core package.
        /// </summary>
        /// <param name="builder">The <see cref="IdentityBuilder"/>.</param>
        /// <param name="configUIOptions">Configures the <see cref="AuthCoreUIOptions"/></param>
        /// <returns>The same <see cref="IdentityBuilder"/> so multiple calls can be chained.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <see cref="ApplicationPartManager"/> service cannot be loaded. The service should be added 
        /// during processing of the .AddDefaultIdentity statement.
        /// </exception>
        public static IdentityBuilder AddAuthorizationCoreUI(this IdentityBuilder builder, Action<AuthCoreUIOptions> configUIOptions)
        {
            var roleType = builder.RoleType ?? typeof(IdentityUserRole<string>);

            builder.Services.Configure(configUIOptions);

            builder.Services.ConfigureOptions(
                typeof(ConfigureAuthCoreUIRazorOptions<,>).MakeGenericType(builder.UserType, roleType)
                );

            var partManager = GetService<ApplicationPartManager>(builder.Services)
                ?? throw new InvalidOperationException($"Could not load {nameof(ApplicationPartManager)} service.");

            TryAddApplicationParts(partManager);

            partManager.FeatureProviders.Add(
                new ViewVersionFeatureProvider(DetermineUIFramework(builder.Services))
                );

            return builder;
        }

        /// <summary>
        /// Retrieves the specified service from the <see cref="IServiceCollection"/>.
        /// <see langword="null"/>, if the requested service is not found.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the requested service.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to be searched.</param>
        /// <returns>The requested service or <see langword="null"/> if not found.</returns>
        private static T? GetService<T>(IServiceCollection services)
            => (T?)services.LastOrDefault(d => d.ServiceType == typeof(T))?.ImplementationInstance;

        /// <summary>
        /// If not already present, adds the <see cref="ApplicationPart"/>s for the Authorization.Core.UI package.
        /// </summary>
        /// <param name="partManager">The <see cref="ApplicationPartManager"/> to be updated.</param>
        /// <returns>
        /// <see langword="true"/>, if the <see cref="ApplicationPart"/>s were added; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool TryAddApplicationParts(ApplicationPartManager partManager)
        {
            var assembly = typeof(IdentityBuilderExtensions).Assembly;
            if (partManager.ApplicationParts.Any(ap => ap.Name == assembly.GetName().Name))
            {
                return false;
            }

            var parts = new ConsolidatedAssemblyApplicationPartFactory().GetApplicationParts(assembly);
            foreach (var part in parts)
            {
                partManager.ApplicationParts.Add(part);
            }

            return true;
        }

        /// <summary>
        /// The UI framework in use by the application.
        /// </summary>
        /// <remarks>
        /// The default version must be assigned to 0.
        /// This enum was copied from Microsoft.AspNetCore.Identity.UI
        /// </remarks>
        internal enum UIFramework
        {
            Bootstrap5 = 0,
            Bootstrap4 = 1
        }

        /// <summary>
        /// Determines the UI framework in use by the application.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> provided by the <see cref="IdentityBuilder"/>.</param>
        /// <returns>The UI framework in use by the application.</returns>
        private static UIFramework DetermineUIFramework(IServiceCollection services)
        {
            /// This logic is based on Microsoft.AspNetCore.Identity.UI
            /// https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityBuilderUIExtensions.cs

            if (!TryResolveUIFramework(Assembly.GetEntryAssembly(), out var framework))
            {
                TryResolveUIFramework(GetApplicationAssembly(services), out framework);
            }

            return framework;
        }

        /// <summary>
        /// Tries to resolve the UIFramework in use by the application.
        /// </summary>
        /// <param name="assembly">The assembly to be checked for a <see cref="UIFrameworkAttribute"/>.</param>
        /// <param name="uiFramework">
        /// Output parameter that will contain the resolved UIFramework.
        /// If this method returns <see langword="false"/>, the parameter will be set to UIFramework.Bootstrap5.
        /// </param>
        /// <returns>
        /// <see langword="true"/>, if the UIFramework is successfully resolved; otherwise <see langword="false"/>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a <see cref="UIFrameworkAttribute"/> with an invalid UIFramework value is found.
        /// </exception>
        private static bool TryResolveUIFramework(Assembly? assembly, out UIFramework uiFramework)
        {
            /// This logic is based on Microsoft.AspNetCore.Identity.UI
            /// https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityBuilderUIExtensions.cs

            uiFramework = default; // Bootstrap5 is the default

            var metadata = assembly?.GetCustomAttributes<UIFrameworkAttribute>()
                .SingleOrDefault()?.UIFramework;
            if (metadata == null)
            {
                return false;
            }

            // If we find the metadata there must be a valid framework here.
            if (!Enum.TryParse(metadata, ignoreCase: true, out uiFramework))
            {
                var enumValues = string.Join(", ", Enum.GetNames(typeof(UIFramework)).Select(v => $"'{v}'"));
                throw new InvalidOperationException(
                    $"Found an invalid value for the 'IdentityUIFrameworkVersion'. Valid values are {enumValues}");
            }

            return true;
        }

        /// <summary>
        /// Returns the Web Application's assembly.
        /// </summary>
        /// <param name="services">
        /// The <see cref="IServiceCollection"/> provided by the <see cref="IdentityBuilder"/>.
        /// </param>
        /// <returns>The Web Application's assembly.</returns>
        private static Assembly? GetApplicationAssembly(IServiceCollection services)
        {
            /// This code is based on Microsoft.AspNetCore.Identity.UI
            /// https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityBuilderUIExtensions.cs

            // This is the same logic that MVC follows to find the application assembly.
            var webHostEnvironment = services.Where(d => d.ServiceType == typeof(IWebHostEnvironment))
                .LastOrDefault()?.ImplementationInstance as IWebHostEnvironment;
            if (webHostEnvironment == null)
            {
                return null;
            }

            var appAssembly = Assembly.Load(webHostEnvironment.ApplicationName);
            return appAssembly;
        }

        /// <summary>
        /// An ApplicationFeatureProvider that selects and configures the appropriate Authorization.Core.UI 
        /// Razor Views.
        /// </summary>
        /// <remarks>
        /// The Authorization.Core.UI package includes two sets of Razor pages; one built with  
        /// Bootstrap version 4 and the other with Bootstrap version 5.
        /// <para>
        /// To use the version 4 pages, you must add an IdentityUIFrameworkVersion parameter that specifies 
        /// "Bootstrap4" to the application's project file (inside a PropertyGroup).
        /// If the parameter is not added to the application's project file, the version 5 pages are used.
        /// </para>
        /// </remarks>
        internal class ViewVersionFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
        {
            /// This code was lifted from Microsoft.AspNetCore.Identity.UI
            /// https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/IdentityBuilderUIExtensions.cs

            private readonly UIFramework _framework;

            public ViewVersionFeatureProvider(UIFramework framework) => _framework = framework;

            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
            {
                var viewsToRemove = new List<CompiledViewDescriptor>();
                foreach (var descriptor in feature.ViewDescriptors)
                {
                    if (IsAuthorizationUIView(descriptor))
                    {
                        switch (_framework)
                        {
                            case UIFramework.Bootstrap4:
                                if (descriptor.Type?.FullName?.Contains("V5") ?? false)
                                {
                                    // Remove V5 views
                                    viewsToRemove.Add(descriptor);
                                }
                                else
                                {
                                    // Fix up paths to eliminate version subdir
                                    descriptor.RelativePath = descriptor.RelativePath.Replace("V4/", "");
                                }
                                break;
                            case UIFramework.Bootstrap5:
                                if (descriptor.Type?.FullName?.Contains("V4") ?? false)
                                {
                                    // Remove V4 views
                                    viewsToRemove.Add(descriptor);
                                }
                                else
                                {
                                    // Fix up paths to eliminate version subdir
                                    descriptor.RelativePath = descriptor.RelativePath.Replace("V5/", "");
                                }
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown framework: {_framework}");
                        }
                    }
                }

                foreach (var descriptorToRemove in viewsToRemove)
                {
                    feature.ViewDescriptors.Remove(descriptorToRemove);
                }
            }

            /// <summary>
            /// Returns an indication of whether the specified <see cref="CompiledViewDescriptor"/> describes 
            /// a Razor View contained in the Authorization.Core.UI assembly.
            /// </summary>
            /// <param name="descriptor">the <see cref="CompiledViewDescriptor"/> to be checked.</param>
            /// <returns>
            /// <see langword="true"/>, if the <paramref name="descriptor"/> describes a Razor View contained in 
            /// the Authorization.Core.UI assembly; otherwise, <see langword="false"/>.
            /// </returns>
            private static bool IsAuthorizationUIView(CompiledViewDescriptor descriptor)
                => descriptor.RelativePath.StartsWith("/Areas/Authorization", StringComparison.OrdinalIgnoreCase) 
                    && descriptor?.Type?.Assembly == typeof(IdentityBuilderExtensions).Assembly;
        }
    }
}
