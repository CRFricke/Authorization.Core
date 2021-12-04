using System;

namespace CRFricke.Authorization.Core.UI
{
    /// <summary>
    /// Specifies the generic type template to be used to instantiate the model for the associated razor page.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class PageImplementationTypeAttribute : Attribute
    {
        public PageImplementationTypeAttribute(Type implementationType)
        {
            Type = implementationType;
        }
        /// <summary>
        /// The generic type template to be used to instantiate the model for the associated razor page.
        /// </summary>
        public Type Type { get; }
    }
}