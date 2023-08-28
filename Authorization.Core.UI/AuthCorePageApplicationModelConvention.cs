using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CRFricke.Authorization.Core.UI;

/// <summary>
/// Generates the correct TypeInfo required to instantiate the model for the specified razor page.
/// </summary>
/// <typeparam name="TUser">The <see cref="Type"/> of user objects.</typeparam>
/// <typeparam name="TRole">The <see cref="Type"/> of role objects.</typeparam>
internal class AuthCorePageApplicationModelConvention<TUser, TRole> : IPageApplicationModelConvention
    where TUser : class
    where TRole : class
{
    [RequiresUnreferencedCode("Call to 'System.Type.MakeGenericType(Type[])' can not be statically analyzed.")]
    public void Apply(PageApplicationModel pam)
    {
        var attribute = pam.ModelType.GetCustomAttribute<PageImplementationTypeAttribute>();
        if (attribute == null)
        {
            return;
        }

        ValidateType(attribute.Type);

        if (attribute.Type.GetGenericArguments().Length == 1)
        {
            var typeArg = pam.ModelType.Namespace.EndsWith(".User") ? typeof(TUser) : typeof(TRole);

            pam.ModelType = attribute.Type.MakeGenericType(typeArg).GetTypeInfo();
            return;
        }

        pam.ModelType = attribute.Type
            .MakeGenericType(typeof(TUser), typeof(TRole))
            .GetTypeInfo();
    }

    private static void ValidateType(Type template)
    {
        if (template.IsAbstract || !template.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException("Implementation type can't be abstract or non generic.");
        }
        var genericArguments = template.GetGenericArguments();
        if (genericArguments.Length < 1 || genericArguments.Length > 2)
        {
            throw new InvalidOperationException("Implementation type contains wrong generic arity.");
        }
    }
}