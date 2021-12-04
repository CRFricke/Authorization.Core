using System;
using System.Collections.Generic;
using System.Linq;

namespace CRFricke.Test.Fakes
{
    /// <summary>
    /// Base class used to track the state of an entity's properties and claims.
    /// </summary>
    public abstract class EntityTrackerBase
    {
        /// <summary>
        /// Defines the properties exposed by the PropertyTracker.
        /// </summary>
        private interface IPropertyTracker
        {
            /// <summary>
            /// The current value of the property, rendered as a string.
            /// </summary>
            string CurrentValue { get; }

            /// <summary>
            /// Returns <c>true</c>, if the value of the property has changed; otherwise, <c>false</c>.
            /// </summary>
            /// <remarks>
            /// Always returns <c>false</c> when the current value is the equal to the initial value of the property,
            /// regardless of how many times the property value was changed.
            /// </remarks>
            bool IsDirty { get; }
        }

        /// <summary>
        /// Tracks the value of a property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="type"/> of the property.</typeparam>
        private struct PropertyTracker<TProperty> : IPropertyTracker where TProperty : IEquatable<TProperty>
        {
            /// <summary>
            /// Creates a new PropertyTracker instance with the specified initial value.
            /// </summary>
            /// <param name="initialValue">The initial value of the property.</param>
            public PropertyTracker(TProperty? initialValue)
            {
                CurrentValue = InitialValue = initialValue;
            }

            /// <summary>
            /// The current value of the property.
            /// </summary>
            public TProperty? CurrentValue { get; internal set; }

            /// <summary>
            /// The initial value of the property.
            /// </summary>
            public TProperty? InitialValue { get; private set; }

            /// <inheritdoc/>
            public bool IsDirty
            {
                get
                {
                    // Both null or the same instance?
                    if (ReferenceEquals(CurrentValue, InitialValue))
                    {
                        return false;
                    }

                    // Either null?
                    if (CurrentValue == null || InitialValue == null)
                    {
                        return true;
                    }

                    return !InitialValue.Equals(CurrentValue);
                }
            }

            /// <inheritdoc/>
            string IPropertyTracker.CurrentValue => CurrentValue?.ToString() ?? "<null>";

            /// <summary>
            /// Returns the current value of the <see cref="PropertyTracker"/>, rendered as a string.
            /// </summary>
            public override string ToString() => CurrentValue?.ToString() ?? "<null>";
        }


        private readonly Dictionary<string, IPropertyTracker> _trackers = new();

        /// <summary>
        /// The entity's original claims.
        /// </summary>
        protected string[]? OriginalClaims { get; set; }

        /// <summary>
        /// The entity's current claims.
        /// </summary>
        protected string[]? CurrentClaims { get; set; }

        /// <summary>
        /// Initializes a property tracker for the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> of the property.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The property's value.</param>
        public virtual void InitPropertyTracker<TProperty>(string key, TProperty? value) where TProperty : IEquatable<TProperty>
        {
            _trackers.Add(key, new PropertyTracker<TProperty>(value));
        }

        /// <summary>
        /// Initializes a property tracker for the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> of the property.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The property's value.</param>
        public virtual void InitPropertyTracker<TProperty>(string key, TProperty? value) where TProperty : struct, IEquatable<TProperty>
        {
            _trackers.Add(key, new PropertyTracker<TProperty>(value ?? default));
        }

        /// <summary>
        /// Returns the current value of the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> of the property.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <returns>The property's current value.</returns>
        public virtual TProperty? GetValue<TProperty>(string key) where TProperty : IEquatable<TProperty>
        {
            if (_trackers.TryGetValue(key, out var tracker))
            {
                if (tracker is PropertyTracker<TProperty> propertyTracker)
                {
                    return propertyTracker.CurrentValue;
                }

                throw new InvalidOperationException($"Tracker for property '{key}' is not of type {typeof(TProperty).Name}.");
            }

            throw new InvalidOperationException($"Tracker for property '{key}' was not initialized.");
        }

        /// <summary>
        /// Sets the value of the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> of the property.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The property's value.</param>
        public virtual void SetValue<TProperty>(string key, object? value) where TProperty : IEquatable<TProperty>
        {
            if (value != null && value is not TProperty)
            {
                throw new ArgumentException($"New property value is not of type {typeof(TProperty).Name}.", nameof(value));
            }

            if (_trackers.TryGetValue(key, out var tracker))
            {
                if (tracker is PropertyTracker<TProperty> propertyTracker)
                {
                    propertyTracker.CurrentValue = (TProperty?)value;
                    _trackers[key] = propertyTracker;
                    return;
                }

                throw new InvalidOperationException($"Tracker for property '{key}' is not of type {typeof(TProperty).Name}.");
            }

            throw new InvalidOperationException($"Tracker for property '{key}' was not initialized.");
        }

        /// <summary>
        /// Attempts to retrieve the value of the specified property.
        /// </summary>
        /// <typeparam name="TProperty">The <see cref="Type"/> of the property.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <param name="value">The output variable to receive the property's value.</param>
        /// <returns><c>true</c>, if the value is successfully retrieved; otherwise, <c>false</c>.</returns>
        public virtual bool TryGetValue<TProperty>(string key, out TProperty? value) where TProperty : IEquatable<TProperty>
        {
            if (_trackers.TryGetValue(key, out var tracker))
            {
                if (tracker is PropertyTracker<TProperty> propertyTracker)
                {
                    value = propertyTracker.CurrentValue;
                    return true;
                }

                throw new InvalidOperationException($"Tracker for property '{key}' is not of type {typeof(TProperty).Name}.");
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Retrieves the current claims assigned to the entity.
        /// </summary>
        /// <returns>The current claims assigned to the entity.</returns>
        public string[] GetCurrentClaims()
            => CurrentClaims ?? new string[] { };

        /// <summary>
        /// Retrieves any updates that have been made to the properties being tracked.
        /// </summary>
        /// <returns>The name and current value of any property that was updated.</returns>
        public IDictionary<string, string> GetUpdates()
        {
            var updates =
                from kvp in _trackers
                where kvp.Value.IsDirty
                select KeyValuePair.Create(kvp.Key, kvp.Value.CurrentValue);

            return new Dictionary<string, string>(updates);
        }
    }
}
