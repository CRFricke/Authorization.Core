using System.Collections.Generic;

namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    public abstract class ContextBase
    {
        private readonly IDictionary<string, object> _properties;

        protected ContextBase() : this(new Dictionary<string, object>())
        { }

        protected ContextBase(ContextBase currentContext)
            : this(new Dictionary<string, object>(currentContext._properties))
        { }

        private ContextBase(IDictionary<string, object> properties)
        {
            _properties = properties;
        }

        protected TValue GetValue<TValue>(string key) =>
            _properties.TryGetValue(key, out var rawValue) ? (TValue)rawValue : default;

        protected void SetValue(string key, object value) =>
            _properties[key] = value;
    }
}