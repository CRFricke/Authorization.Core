using Microsoft.Extensions.Caching.Memory;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// A local in-memory cache used to associate ApplicationRoles with ApplicationUsers.
    /// </summary>
    internal class UserRoleCache : IMemoryCache
    {
        readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        ///<inheritdoc/>
        public virtual ICacheEntry CreateEntry(object key) => _cache.CreateEntry(key);

        ///<inheritdoc/>
        public virtual void Dispose() => _cache.Dispose();

        ///<inheritdoc/>
        public virtual void Remove(object key) => _cache.Remove(key);

        ///<inheritdoc/>
        public virtual bool TryGetValue(object key, out object? value) => _cache.TryGetValue(key, out value);
    }
}
